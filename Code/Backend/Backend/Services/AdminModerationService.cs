using Backend.Dtos.Admin;
using Backend.Models;
using Backend.Models.Enums;
using Backend.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services;

public class AdminModerationService : IAdminModerationService
{
    private const string None = "none";
    private const string RemoveProduct = "remove_product";
    private const string RestoreProduct = "restore_product";
    private const string BanUser = "ban_user";
    private const string MuteUser = "mute_user";
    private const string RestrictPublish = "restrict_publish";
    private const string UnbanUser = "unban_user";
    private const string WarnUser = "warn_user";

    private readonly IWorkOrderRepository _workOrderRepo;
    private readonly IWorkOrderTimelineRepository _timelineRepo;
    private readonly IProductRepository _productRepo;
    private readonly IProductAuditLogRepository _auditRepo;
    private readonly IBaseUserRepository _baseUserRepo;
    private readonly IUserWarningRepository _warningRepo;

    private static readonly HashSet<string> AllowedHandleActions = new(StringComparer.OrdinalIgnoreCase)
    {
        None, RemoveProduct, RestoreProduct, BanUser, MuteUser, RestrictPublish, UnbanUser, WarnUser
    };

    public AdminModerationService(
        IWorkOrderRepository workOrderRepo,
        IWorkOrderTimelineRepository timelineRepo,
        IProductRepository productRepo,
        IProductAuditLogRepository auditRepo,
        IBaseUserRepository baseUserRepo,
        IUserWarningRepository warningRepo)
    {
        _workOrderRepo = workOrderRepo;
        _timelineRepo = timelineRepo;
        _productRepo = productRepo;
        _auditRepo = auditRepo;
        _baseUserRepo = baseUserRepo;
        _warningRepo = warningRepo;
    }

    public async Task<AdminModerationPageDto> GetReportsAsync(
        string? keyword,
        string? status,
        string? targetType,
        int page,
        int pageSize)
        => await GetPageAsync((int)WorkOrderType.Report, keyword, status, targetType, page, pageSize);

    public async Task<AdminModerationPageDto> GetAppealsAsync(
        string? keyword,
        string? status,
        string? targetType,
        int page,
        int pageSize)
        => await GetPageAsync((int)WorkOrderType.Appeal, keyword, status, targetType, page, pageSize);

    public async Task<AdminModerationDetailDto?> GetReportDetailAsync(long reportId)
        => await GetDetailAsync(reportId, (int)WorkOrderType.Report);

    public async Task<AdminModerationDetailDto?> GetAppealDetailAsync(long appealId)
        => await GetDetailAsync(appealId, (int)WorkOrderType.Appeal);

    public async Task<AdminModerationDetailDto?> AcceptReportAsync(long reportId, int adminId)
    {
        var workOrder = await GetWorkOrderForActionAsync(reportId, (int)WorkOrderType.Report);
        if (workOrder == null) return null;

        workOrder.Status = "done";
        workOrder.Result = "accepted";
        workOrder.AdminId = adminId;
        workOrder.ResponseTime = DateTime.Now;

        await AddTimelineAsync(reportId, "accept", "举报成立", adminId);
        await _workOrderRepo.SaveAsync();
        return await GetReportDetailAsync(reportId);
    }

    public async Task<AdminModerationDetailDto?> RejectReportAsync(long reportId, int adminId)
    {
        var workOrder = await GetWorkOrderForActionAsync(reportId, (int)WorkOrderType.Report);
        if (workOrder == null) return null;

        workOrder.Status = "done";
        workOrder.Result = "rejected";
        workOrder.AdminId = adminId;
        workOrder.ResponseTime = DateTime.Now;

        await AddTimelineAsync(reportId, "reject", "举报不成立", adminId);
        await _workOrderRepo.SaveAsync();
        return await GetReportDetailAsync(reportId);
    }

    public async Task<AdminModerationDetailDto?> HandleReportAsync(
        long reportId,
        HandleWorkOrderDto dto,
        int adminId)
    {
        var workOrder = await GetWorkOrderForActionAsync(reportId, (int)WorkOrderType.Report);
        if (workOrder == null) return null;

        if (!AllowedHandleActions.Contains(dto.Action.Trim()))
            throw new ArgumentException("不支持的处理动作");

        await ApplyHandleActionAsync(workOrder, dto.Action.Trim(), dto.Reason.Trim(), adminId);

        workOrder.Status = "done";
        workOrder.Result = "handled";
        workOrder.HandleAction = dto.Action.Trim();
        workOrder.Response = dto.Reason.Trim();
        workOrder.AdminId = adminId;
        workOrder.ResponseTime = DateTime.Now;

        await AddTimelineAsync(reportId, "handle", dto.Reason.Trim(), adminId);
        await _workOrderRepo.SaveAsync();
        return await GetReportDetailAsync(reportId);
    }

    public async Task<AdminModerationDetailDto?> ApproveAppealAsync(long appealId, int adminId)
    {
        var workOrder = await GetWorkOrderForActionAsync(appealId, (int)WorkOrderType.Appeal);
        if (workOrder == null) return null;

        var reversal = GetReversalAction(workOrder.AppealAgainst?.HandleAction);
        if (reversal != None)
            await ApplyHandleActionAsync(workOrder, reversal, "申诉通过，撤销原处理", adminId);

        workOrder.Status = "done";
        workOrder.Result = "approved";
        workOrder.HandleAction = reversal;
        workOrder.AdminId = adminId;
        workOrder.ResponseTime = DateTime.Now;

        await AddTimelineAsync(appealId, "approve", "申诉通过", adminId);
        await _workOrderRepo.SaveAsync();
        return await GetAppealDetailAsync(appealId);
    }

    public async Task<AdminModerationDetailDto?> RejectAppealAsync(long appealId, int adminId)
    {
        var workOrder = await GetWorkOrderForActionAsync(appealId, (int)WorkOrderType.Appeal);
        if (workOrder == null) return null;

        workOrder.Status = "done";
        workOrder.Result = "rejected";
        workOrder.AdminId = adminId;
        workOrder.ResponseTime = DateTime.Now;

        await AddTimelineAsync(appealId, "reject", "申诉驳回", adminId);
        await _workOrderRepo.SaveAsync();
        return await GetAppealDetailAsync(appealId);
    }

    public async Task<AdminModerationDetailDto?> ReplyAppealAsync(
        long appealId,
        WorkOrderReplyDto dto,
        int adminId)
    {
        var workOrder = await GetWorkOrderForActionAsync(appealId, (int)WorkOrderType.Appeal);
        if (workOrder == null) return null;

        workOrder.Response = dto.Reply.Trim();
        workOrder.ResponseTime = DateTime.Now;
        workOrder.AdminId = adminId;

        await AddTimelineAsync(appealId, "reply", dto.Reply.Trim(), adminId);
        await _workOrderRepo.SaveAsync();
        return await GetAppealDetailAsync(appealId);
    }

    public async Task<AdminModerationTasksDto> GetTasksAsync()
    {
        var reports = await _workOrderRepo.GetAdminPageAsync((int)WorkOrderType.Report, null, null, null, 1, 20);
        var appeals = await _workOrderRepo.GetAdminPageAsync((int)WorkOrderType.Appeal, null, null, null, 1, 20);
        var all = reports.Items.Concat(appeals.Items)
            .OrderByDescending(w => w.CreateTime)
            .Take(20)
            .ToList();

        return new AdminModerationTasksDto
        {
            WaitingCount = await _workOrderRepo.Query().CountAsync(w => w.Status == "waiting"),
            ProcessingCount = await _workOrderRepo.Query().CountAsync(w => w.Status == "processing"),
            ReportCount = reports.Total,
            AppealCount = appeals.Total,
            TotalPending = await _workOrderRepo.Query().CountAsync(w => w.Status != "done"),
            RecentTasks = all.Select(ToListItem).ToList()
        };
    }

    private async Task<AdminModerationPageDto> GetPageAsync(
        int type,
        string? keyword,
        string? status,
        string? targetType,
        int page,
        int pageSize)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 20 : pageSize;
        pageSize = pageSize > 100 ? 100 : pageSize;

        var (items, total) = await _workOrderRepo.GetAdminPageAsync(
            type, keyword, status, targetType, page, pageSize);

        return new AdminModerationPageDto
        {
            Items = items.Select(ToListItem).ToList(),
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        };
    }

    private async Task<AdminModerationDetailDto?> GetDetailAsync(long workOrderId, int expectedType)
    {
        var workOrder = await _workOrderRepo.GetDetailAsync(workOrderId);
        if (workOrder == null || workOrder.Type != expectedType) return null;

        var dto = ToListItem(workOrder);
        var timeline = await _timelineRepo.GetByWorkOrderIdAsync(workOrderId);

        return new AdminModerationDetailDto
        {
            WorkOrderId = dto.WorkOrderId,
            Type = dto.Type,
            TargetType = dto.TargetType,
            TargetId = dto.TargetId,
            Reason = dto.Reason,
            Info = dto.Info,
            Status = dto.Status,
            Result = dto.Result,
            HandleAction = dto.HandleAction,
            CreateTime = dto.CreateTime,
            Response = dto.Response,
            ResponseTime = dto.ResponseTime,
            InitiatorId = dto.InitiatorId,
            InitiatorName = dto.InitiatorName,
            AccusedId = dto.AccusedId,
            AccusedName = dto.AccusedName,
            ProductId = dto.ProductId,
            ProductName = dto.ProductName,
            AppealAgainstWorkOrderId = dto.AppealAgainstWorkOrderId,
            AppealAgainstReason = dto.AppealAgainstReason,
            AdminId = dto.AdminId,
            Timeline = timeline.Select(t => new AdminWorkOrderTimelineDto
            {
                TimelineId = t.TimelineId,
                Action = t.Action,
                Note = t.Note,
                AdminId = t.AdminId,
                CreateTime = t.CreateTime
            }).ToList()
        };
    }

    private async Task<WorkOrder?> GetWorkOrderForActionAsync(long workOrderId, int expectedType)
    {
        var workOrder = await _workOrderRepo.GetDetailAsync(workOrderId);
        if (workOrder == null || workOrder.Type != expectedType) return null;
        if (workOrder.Status == "done") throw new InvalidOperationException("该工单已处理完成");
        return workOrder;
    }

    private async Task ApplyHandleActionAsync(WorkOrder workOrder, string action, string reason, int adminId)
    {
        switch (action)
        {
            case RemoveProduct:
                if (workOrder.ProductId == null) throw new InvalidOperationException("该工单未关联商品，无法执行下架");
                await SetProductStatusAsync(workOrder.ProductId.Value, ProductStatus.Removed, "remove", reason, adminId);
                break;

            case RestoreProduct:
                if (workOrder.ProductId == null) throw new InvalidOperationException("该工单未关联商品，无法恢复");
                await SetProductStatusAsync(workOrder.ProductId.Value, ProductStatus.Available, "restore", reason, adminId);
                break;

            case BanUser:
                if (workOrder.AccusedId == null) throw new InvalidOperationException("该工单未关联用户，无法封禁");
                await SetAccountStatusAsync(workOrder.AccusedId.Value, AccountStatus.Banned);
                break;

            case MuteUser:
                if (workOrder.AccusedId == null) throw new InvalidOperationException("该工单未关联用户，无法禁言");
                await SetAccountStatusAsync(workOrder.AccusedId.Value, AccountStatus.Muted);
                break;

            case RestrictPublish:
                if (workOrder.AccusedId == null) throw new InvalidOperationException("该工单未关联用户，无法限制发布");
                await SetAccountStatusAsync(workOrder.AccusedId.Value, AccountStatus.PublishRestricted);
                break;

            case UnbanUser:
                if (workOrder.AccusedId == null) throw new InvalidOperationException("该工单未关联用户，无法解除限制");
                await SetAccountStatusAsync(workOrder.AccusedId.Value, AccountStatus.Normal);
                break;

            case WarnUser:
                if (workOrder.AccusedId == null) throw new InvalidOperationException("该工单未关联用户，无法发送警告");
                await _warningRepo.AddAsync(new UserWarning
                {
                    UserId = workOrder.AccusedId.Value,
                    AdminId = adminId,
                    Reason = reason,
                    CreateTime = DateTime.Now
                });
                break;
        }
    }

    private async Task SetProductStatusAsync(long productId, ProductStatus status, string auditAction, string reason, int adminId)
    {
        var product = await _productRepo.GetByIdAsync(productId);
        if (product == null) throw new InvalidOperationException("关联商品不存在");

        var oldStatus = product.Status;
        product.Status = status;
        _productRepo.Update(product);

        await _auditRepo.AddAsync(new ProductAuditLog
        {
            ProductId = product.ProductId,
            AdminId = adminId,
            Action = auditAction,
            Reason = reason,
            OldStatus = oldStatus,
            NewStatus = status,
            CreateTime = DateTime.Now
        });
    }

    private async Task SetAccountStatusAsync(int userId, AccountStatus status)
    {
        var user = await _baseUserRepo.GetByIdWithProfileAsync(userId);
        if (user == null) throw new InvalidOperationException("关联用户不存在");

        user.AccountStatus = status;
        user.IsBanned = status == AccountStatus.Banned ? 1 : 0;
        user.BannedUntil = status == AccountStatus.Banned ? null : null;
        _baseUserRepo.Update(user);
    }

    private static string GetReversalAction(string? handleAction)
        => handleAction switch
        {
            RemoveProduct => RestoreProduct,
            BanUser or MuteUser or RestrictPublish => UnbanUser,
            _ => None
        };

    private async Task AddTimelineAsync(long workOrderId, string action, string? note, int adminId)
    {
        await _timelineRepo.AddAsync(new WorkOrderTimeline
        {
            WorkOrderId = workOrderId,
            Action = action,
            Note = note,
            AdminId = adminId,
            CreateTime = DateTime.Now
        });
    }

    private static AdminModerationWorkOrderDto ToListItem(WorkOrder w) => new()
    {
        WorkOrderId = w.WorkOrderId,
        Type = w.Type,
        TargetType = w.TargetType,
        TargetId = w.TargetId,
        Reason = w.Reason,
        Info = w.Info,
        Status = w.Status,
        Result = w.Result,
        HandleAction = w.HandleAction,
        CreateTime = w.CreateTime,
        Response = w.Response,
        ResponseTime = w.ResponseTime,
        InitiatorId = w.InitiatorId,
        InitiatorName = w.Initiator?.UserName ?? "",
        AccusedId = w.AccusedId,
        AccusedName = w.Accused?.UserName,
        ProductId = w.ProductId,
        ProductName = w.Product?.Name,
        AppealAgainstWorkOrderId = w.AppealAgainstWorkOrderId,
        AppealAgainstReason = w.AppealAgainst?.Reason,
        AdminId = w.AdminId
    };
}

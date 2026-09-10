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
    private const string Approve = "approve";

    private readonly IWorkOrderRepository _workOrderRepo;
    private readonly IWorkOrderTimelineRepository _timelineRepo;
    private readonly IProductRepository _productRepo;
    private readonly IProductAuditLogRepository _auditRepo;
    private readonly IBaseUserRepository _baseUserRepo;
    private readonly IUserWarningRepository _warningRepo;
    private readonly IReputationService _reputationService;

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
        IUserWarningRepository warningRepo,
        IReputationService reputationService)
    {
        _workOrderRepo = workOrderRepo;
        _timelineRepo = timelineRepo;
        _productRepo = productRepo;
        _auditRepo = auditRepo;
        _baseUserRepo = baseUserRepo;
        _warningRepo = warningRepo;
        _reputationService = reputationService;
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

    public async Task<AdminModerationPageDto> GetWorkOrdersAsync(
        string? type,
        string? keyword,
        string? status,
        string? targetType,
        int page,
        int pageSize)
        => await GetPageAsync(ParseWorkOrderType(type), keyword, status, targetType, page, pageSize);

    public async Task<AdminModerationDetailDto?> GetReportDetailAsync(long reportId)
        => await GetDetailAsync(reportId, (int)WorkOrderType.Report);

    public async Task<AdminModerationDetailDto?> GetAppealDetailAsync(long appealId)
        => await GetDetailAsync(appealId, (int)WorkOrderType.Appeal);

    public async Task<AdminModerationDetailDto?> GetWorkOrderDetailAsync(long workOrderId)
        => await GetDetailAsync(workOrderId, null);

    public async Task<AdminModerationDetailDto?> AcceptReportAsync(long reportId, int adminId)
    {
        var workOrder = await GetWorkOrderForActionAsync(reportId, (int)WorkOrderType.Report);
        if (workOrder == null) return null;

        workOrder.Status = "done";
        workOrder.Result = "accepted";
        workOrder.AdminId = adminId;
        workOrder.ResponseTime = DateTime.Now;

        var accusedId = ResolveTargetUserId(workOrder);
        if (accusedId.HasValue)
            await _reputationService.ChangeCreditAsync(accusedId.Value, CreditRules.ReportAccepted);

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

        var reversal = GetAppealResolutionAction(workOrder);
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

        await _reputationService.ChangeCreditAsync(workOrder.InitiatorId, CreditRules.AppealRejected);

        await AddTimelineAsync(appealId, "reject", "申诉驳回", adminId);
        await _workOrderRepo.SaveAsync();
        return await GetAppealDetailAsync(appealId);
    }

    public async Task<AdminModerationDetailDto?> RejectWorkOrderAsync(long workOrderId, int adminId)
    {
        var workOrder = await GetWorkOrderForActionAsync(workOrderId);
        if (workOrder == null) return null;

        workOrder.Status = "done";
        workOrder.Result = "rejected";
        workOrder.AdminId = adminId;
        workOrder.ResponseTime = DateTime.Now;

        var note = workOrder.Type == (int)WorkOrderType.Report ? "举报不成立" : "申诉驳回";
        await AddTimelineAsync(workOrderId, "reject", note, adminId);
        await _workOrderRepo.SaveAsync();
        return await GetWorkOrderDetailAsync(workOrderId);
    }

    public async Task<AdminModerationDetailDto?> ProcessWorkOrderAsync(
        long workOrderId,
        HandleWorkOrderDto dto,
        int adminId)
    {
        var workOrder = await GetWorkOrderForActionAsync(workOrderId);
        if (workOrder == null) return null;

        if (workOrder.Type == (int)WorkOrderType.Report)
        {
            await ProcessReportInternalAsync(workOrder, dto, adminId);
            return await GetWorkOrderDetailAsync(workOrderId);
        }

        if (workOrder.Type == (int)WorkOrderType.Appeal)
        {
            await ProcessAppealInternalAsync(workOrder, dto, adminId);
            return await GetWorkOrderDetailAsync(workOrderId);
        }

        throw new ArgumentException("不支持的工单类型");
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
            RecentTasks = all.Select(w => new AdminModerationTaskDto
            {
                Id = w.WorkOrderId,
                Type = w.Type == (int)WorkOrderType.Report ? "report" : "appeal",
                Title = w.Reason,
                Status = w.Status,
                CreateTime = w.CreateTime
            }).ToList()
        };
    }

    private async Task<AdminModerationPageDto> GetPageAsync(
        int? type,
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

    private async Task<AdminModerationDetailDto?> GetDetailAsync(long workOrderId, int? expectedType)
    {
        var workOrder = await _workOrderRepo.GetDetailAsync(workOrderId);
        if (workOrder == null || (expectedType.HasValue && workOrder.Type != expectedType.Value)) return null;

        var dto = ToListItem(workOrder);
        var timeline = await _timelineRepo.GetByWorkOrderIdAsync(workOrderId);
        var (_, attachments) = ExtractAttachments(workOrder.Info);

        return new AdminModerationDetailDto
        {
            WorkOrderId = dto.WorkOrderId,
            ReportId = dto.ReportId,
            AppealId = dto.AppealId,
            Type = dto.Type,
            TargetType = dto.TargetType,
            TargetId = dto.TargetId,
            TargetName = dto.TargetName,
            Reason = dto.Reason,
            Info = dto.Info,
            Description = dto.Description,
            Content = dto.Content,
            Status = dto.Status,
            Result = dto.Result,
            HandleAction = dto.HandleAction,
            CreateTime = dto.CreateTime,
            Response = dto.Response,
            ResponseTime = dto.ResponseTime,
            InitiatorId = dto.InitiatorId,
            InitiatorName = dto.InitiatorName,
            ReporterName = dto.ReporterName,
            UserName = dto.UserName,
            AccusedId = dto.AccusedId,
            AccusedName = dto.AccusedName,
            ProductId = dto.ProductId,
            ProductName = dto.ProductName,
            AppealAgainstWorkOrderId = dto.AppealAgainstWorkOrderId,
            AppealAgainstReason = dto.AppealAgainstReason,
            AdminId = dto.AdminId,
            Attachments = attachments,
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

    private async Task ProcessReportInternalAsync(WorkOrder workOrder, HandleWorkOrderDto dto, int adminId)
    {
        if (!AllowedHandleActions.Contains(dto.Action.Trim()))
            throw new ArgumentException("不支持的处理动作");

        await ApplyHandleActionAsync(workOrder, dto.Action.Trim(), dto.Reason.Trim(), adminId);

        workOrder.Status = "done";
        workOrder.Result = "handled";
        workOrder.HandleAction = dto.Action.Trim();
        workOrder.Response = dto.Reason.Trim();
        workOrder.AdminId = adminId;
        workOrder.ResponseTime = DateTime.Now;

        await AddTimelineAsync(workOrder.WorkOrderId, "handle", dto.Reason.Trim(), adminId);
        await _workOrderRepo.SaveAsync();
    }

    private async Task ProcessAppealInternalAsync(WorkOrder workOrder, HandleWorkOrderDto dto, int adminId)
    {
        if (!string.Equals(dto.Action.Trim(), Approve, StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("申诉处理动作仅支持 approve");

        var reversal = GetAppealResolutionAction(workOrder);
        if (reversal != None)
            await ApplyHandleActionAsync(workOrder, reversal, dto.Reason.Trim(), adminId);

        workOrder.Status = "done";
        workOrder.Result = "approved";
        workOrder.HandleAction = reversal;
        workOrder.Response = dto.Reason.Trim();
        workOrder.AdminId = adminId;
        workOrder.ResponseTime = DateTime.Now;

        await AddTimelineAsync(workOrder.WorkOrderId, "handle", dto.Reason.Trim(), adminId);
        await _workOrderRepo.SaveAsync();
    }

    private static int? ParseWorkOrderType(string? type)
        => type?.Trim().ToLowerInvariant() switch
        {
            null or "" => null,
            "report" => (int)WorkOrderType.Report,
            "appeal" => (int)WorkOrderType.Appeal,
            _ => throw new ArgumentException("不支持的工单类型")
        };

    private async Task<WorkOrder?> GetWorkOrderForActionAsync(long workOrderId, int expectedType)
    {
        var workOrder = await _workOrderRepo.GetDetailAsync(workOrderId);
        if (workOrder == null || workOrder.Type != expectedType) return null;
        if (workOrder.Status == "done") throw new InvalidOperationException("该工单已处理完成");
        return workOrder;
    }

    private async Task<WorkOrder?> GetWorkOrderForActionAsync(long workOrderId)
    {
        var workOrder = await _workOrderRepo.GetDetailAsync(workOrderId);
        if (workOrder == null) return null;
        if (workOrder.Status == "done") throw new InvalidOperationException("该工单已处理完成");
        return workOrder;
    }

    private async Task ApplyHandleActionAsync(WorkOrder workOrder, string action, string reason, int adminId)
    {
        switch (action)
        {
            case RemoveProduct:
                var removedProductId = ResolveTargetProductId(workOrder);
                if (removedProductId == null) throw new InvalidOperationException("该工单未关联商品，无法执行下架");
                await SetProductStatusAsync(removedProductId.Value, ProductStatus.Removed, "remove", reason, adminId);
                break;

            case RestoreProduct:
                var restoredProductId = ResolveTargetProductId(workOrder);
                if (restoredProductId == null) throw new InvalidOperationException("该工单未关联商品，无法恢复");
                await SetProductStatusAsync(restoredProductId.Value, ProductStatus.Available, "restore", reason, adminId);
                break;

            case BanUser:
                var bannedUserId = ResolveTargetUserId(workOrder);
                if (bannedUserId == null) throw new InvalidOperationException("该工单未关联用户，无法封禁");
                await _reputationService.ChangeCreditAsync(bannedUserId.Value, CreditRules.AccountPenalty);
                await SetAccountStatusAsync(bannedUserId.Value, AccountStatus.Banned);
                break;

            case MuteUser:
                var mutedUserId = ResolveTargetUserId(workOrder);
                if (mutedUserId == null) throw new InvalidOperationException("该工单未关联用户，无法禁言");
                await _reputationService.ChangeCreditAsync(mutedUserId.Value, CreditRules.AccountPenalty);
                await SetAccountStatusAsync(mutedUserId.Value, AccountStatus.Muted);
                break;

            case RestrictPublish:
                var restrictedUserId = ResolveTargetUserId(workOrder);
                if (restrictedUserId == null) throw new InvalidOperationException("该工单未关联用户，无法限制发布");
                await _reputationService.ChangeCreditAsync(restrictedUserId.Value, CreditRules.AccountPenalty);
                await SetAccountStatusAsync(restrictedUserId.Value, AccountStatus.PublishRestricted);
                break;

            case UnbanUser:
                var unbannedUserId = ResolveTargetUserId(workOrder);
                if (unbannedUserId == null) throw new InvalidOperationException("该工单未关联用户，无法解除限制");
                await SetAccountStatusAsync(unbannedUserId.Value, AccountStatus.Normal);
                break;

            case WarnUser:
                var warnedUserId = ResolveTargetUserId(workOrder);
                if (warnedUserId == null) throw new InvalidOperationException("该工单未关联用户，无法发送警告");
                await _reputationService.ChangeCreditAsync(warnedUserId.Value, CreditRules.Warned);
                await _warningRepo.AddAsync(new UserWarning
                {
                    UserId = warnedUserId.Value,
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

    private static string GetAppealResolutionAction(WorkOrder workOrder)
    {
        if (string.Equals(workOrder.TargetType, "product", StringComparison.OrdinalIgnoreCase))
            return RestoreProduct;
        if (string.Equals(workOrder.TargetType, "user", StringComparison.OrdinalIgnoreCase))
            return UnbanUser;
        return GetReversalAction(workOrder.AppealAgainst?.HandleAction);
    }

    private static long? ResolveTargetProductId(WorkOrder workOrder)
        => workOrder.ProductId
           ?? (string.Equals(workOrder.TargetType, "product", StringComparison.OrdinalIgnoreCase)
               ? workOrder.TargetId
               : null);

    private static int? ResolveTargetUserId(WorkOrder workOrder)
    {
        if (workOrder.AccusedId.HasValue)
            return workOrder.AccusedId.Value;

        if (string.Equals(workOrder.TargetType, "user", StringComparison.OrdinalIgnoreCase) &&
            workOrder.TargetId is long userId &&
            userId > 0 &&
            userId <= int.MaxValue)
            return (int)userId;

        return null;
    }

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

    private static AdminModerationWorkOrderDto ToListItem(WorkOrder w)
    {
        var (cleanInfo, _) = ExtractAttachments(w.Info);
        return new AdminModerationWorkOrderDto
        {
            WorkOrderId = w.WorkOrderId,
            ReportId = w.WorkOrderId,
            AppealId = w.WorkOrderId,
            Type = w.Type,
            TargetType = w.TargetType,
            TargetId = w.TargetId,
            TargetName = ResolveTargetName(w),
            Reason = w.Reason,
            Info = cleanInfo,
            Description = cleanInfo,
            Content = string.IsNullOrWhiteSpace(cleanInfo) ? w.Reason : cleanInfo,
            Status = w.Status,
            Result = w.Result,
            HandleAction = w.HandleAction,
            CreateTime = w.CreateTime,
            Response = w.Response,
            ResponseTime = w.ResponseTime,
            InitiatorId = w.InitiatorId,
            InitiatorName = w.Initiator?.UserName ?? "",
            ReporterName = w.Initiator?.UserName ?? "",
            UserName = w.Initiator?.UserName ?? "",
            AccusedId = w.AccusedId,
            AccusedName = w.Accused?.UserName,
            ProductId = w.ProductId,
            ProductName = w.Product?.Name,
            AppealAgainstWorkOrderId = w.AppealAgainstWorkOrderId,
            AppealAgainstReason = w.AppealAgainst?.Reason,
            AdminId = w.AdminId
        };
    }

    private static (string? Info, List<AdminWorkOrderAttachmentDto> Attachments) ExtractAttachments(string? info)
    {
        var attachments = new List<AdminWorkOrderAttachmentDto>();
        if (string.IsNullOrWhiteSpace(info)) return (info, attachments);

        var lines = info.Split('\n').ToList();
        for (var i = lines.Count - 1; i >= 0; i--)
        {
            var line = lines[i].Trim();
            if (!line.StartsWith("[附件:", StringComparison.Ordinal) || !line.EndsWith("]"))
                continue;

            var inner = line[4..^1];
            var separator = inner.IndexOf(':');
            if (separator <= 0 || !long.TryParse(inner[..separator], out var fileId))
                continue;

            var fileName = inner[(separator + 1)..];
            if (string.IsNullOrWhiteSpace(fileName))
                continue;

            attachments.Add(new AdminWorkOrderAttachmentDto
            {
                FileId = fileId,
                FileName = fileName
            });
            lines.RemoveAt(i);
        }

        var cleanInfo = string.Join("\n", lines).Trim();
        return (string.IsNullOrEmpty(cleanInfo) ? null : cleanInfo, attachments);
    }

    private static string? ResolveTargetName(WorkOrder w)
    {
        if (!string.IsNullOrEmpty(w.TargetType))
        {
            if (w.TargetType.Equals("product", StringComparison.OrdinalIgnoreCase))
                return w.Product?.Name;
            if (w.TargetType.Equals("user", StringComparison.OrdinalIgnoreCase))
                return w.Accused?.UserName;
        }

        return w.Product?.Name ?? w.Accused?.UserName;
    }
}

using Backend.Dtos.Admin;
using Backend.Dtos.Product;
using Backend.Models;
using Backend.Models.Enums;
using Backend.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services;

public class AdminUserManagementService : IAdminUserManagementService
{
    private readonly IBaseUserRepository _baseUserRepo;
    private readonly IProductRepository _productRepo;
    private readonly IPurchaseRepository _purchaseRepo;
    private readonly IWorkOrderRepository _workOrderRepo;
    private readonly IUserWarningRepository _warningRepo;
    private readonly IReputationService _reputationService;
    private readonly IProductService _productService;
    private readonly IPurchaseService _purchaseService;

    public AdminUserManagementService(
        IBaseUserRepository baseUserRepo,
        IProductRepository productRepo,
        IPurchaseRepository purchaseRepo,
        IWorkOrderRepository workOrderRepo,
        IUserWarningRepository warningRepo,
        IReputationService reputationService,
        IProductService productService,
        IPurchaseService purchaseService)
    {
        _baseUserRepo = baseUserRepo;
        _productRepo = productRepo;
        _purchaseRepo = purchaseRepo;
        _workOrderRepo = workOrderRepo;
        _warningRepo = warningRepo;
        _reputationService = reputationService;
        _productService = productService;
        _purchaseService = purchaseService;
    }

    public async Task<AdminUserPageDto> GetUsersAsync(
        string? keyword,
        int? userType,
        int? accountStatus,
        int? creditMin,
        int? creditMax,
        DateTime? registerStart,
        DateTime? registerEnd,
        int page,
        int pageSize)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 20 : pageSize;
        pageSize = pageSize > 100 ? 100 : pageSize;

        var (items, total) = await _baseUserRepo.GetAdminPageAsync(
            keyword, userType, accountStatus, creditMin, creditMax, registerStart, registerEnd, page, pageSize);

        var userIds = items.Select(u => u.UserId).ToList();

        var productCounts = await _productRepo.Query()
            .Where(p => userIds.Contains(p.UserId))
            .GroupBy(p => p.UserId)
            .Select(g => new { UserId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.UserId, x => x.Count);

        var orderRows = await (from p in _purchaseRepo.Query()
                               join pr in _productRepo.Query() on p.ProductId equals pr.ProductId
                               where userIds.Contains(p.BuyerId) || userIds.Contains(pr.UserId)
                               select new { p.PurchaseId, p.BuyerId, SellerId = pr.UserId }).ToListAsync();

        var orderCounts = orderRows
            .SelectMany(x => new[]
            {
                new { x.PurchaseId, UserId = x.BuyerId },
                new { x.PurchaseId, UserId = x.SellerId }
            })
            .GroupBy(x => x.UserId)
            .ToDictionary(g => g.Key, g => g.Select(x => x.PurchaseId).Distinct().Count());

        var warningCounts = await _warningRepo.Query()
            .Where(w => userIds.Contains(w.UserId))
            .GroupBy(w => w.UserId)
            .Select(g => new { UserId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.UserId, x => x.Count);

        var violationCounts = await _workOrderRepo.Query()
            .Where(w => w.Type == (int)WorkOrderType.Report
                        && w.AccusedId != null
                        && userIds.Contains(w.AccusedId.Value))
            .GroupBy(w => w.AccusedId!.Value)
            .Select(g => new { UserId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.UserId, x => x.Count);

        var result = items.Select(u => ToListItem(
            u,
            productCounts,
            orderCounts,
            warningCounts,
            violationCounts)).ToList();

        return new AdminUserPageDto
        {
            Items = result,
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<AdminUserDetailDto?> GetUserDetailAsync(int userId)
    {
        var user = await _baseUserRepo.GetByIdWithProfileAsync(userId);
        if (user == null) return null;

        var productCount = await _productRepo.Query().CountAsync(p => p.UserId == userId);
        var orderCount = await (from p in _purchaseRepo.Query()
                                join pr in _productRepo.Query() on p.ProductId equals pr.ProductId
                                where p.BuyerId == userId || pr.UserId == userId
                                select p.PurchaseId).Distinct().CountAsync();
        var warningCount = await _warningRepo.Query().CountAsync(w => w.UserId == userId);
        var violationCount = await _workOrderRepo.Query()
            .CountAsync(w => w.Type == (int)WorkOrderType.Report && w.AccusedId == userId);

        return new AdminUserDetailDto
        {
            UserId = user.UserId,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            UserName = user.NormUser?.UserName ?? (user.UserType == 1 ? "管理员" : ""),
            UserType = user.UserType,
            AccountStatus = user.AccountStatus,
            IsBanned = user.IsBanned,
            BannedUntil = user.BannedUntil,
            Credit = user.NormUser?.Credit ?? 0,
            RegisterTime = user.RegisterTime,
            Gender = user.Gender,
            Profile = user.NormUser?.Profile,
            AvatarFileId = user.AvatarFileId,
            ProductCount = productCount,
            OrderCount = orderCount,
            WarningCount = warningCount,
            ViolationCount = violationCount
        };
    }

    public async Task<List<ProductDto>?> GetUserProductsAsync(int userId)
    {
        if (await _baseUserRepo.GetByIdAsync(userId) == null) return null;
        return await _productService.GetProductsByUserIdAsync(userId);
    }

    public async Task<List<PurchaseDto>?> GetUserOrdersAsync(int userId)
    {
        if (await _baseUserRepo.GetByIdAsync(userId) == null) return null;
        return await _purchaseService.GetRelatedByUserIdAsync(userId);
    }

    public async Task<List<AdminWorkOrderDto>?> GetUserReportsAsync(int userId)
    {
        if (await _baseUserRepo.GetByIdAsync(userId) == null) return null;

        var workOrders = await _workOrderRepo.GetRelatedByUserIdAsync(
            userId, (int)WorkOrderType.Report, null, includeAccused: true);

        return workOrders.Select(ToWorkOrderDto).ToList();
    }

    public async Task<List<AdminWorkOrderDto>?> GetUserAppealsAsync(int userId)
    {
        if (await _baseUserRepo.GetByIdAsync(userId) == null) return null;

        var workOrders = await _workOrderRepo.GetRelatedByUserIdAsync(
            userId, (int)WorkOrderType.Appeal, null, includeAccused: false);

        return workOrders.Select(ToWorkOrderDto).ToList();
    }

    public async Task<AdminUserReputationDto> GetUserReputationAsync(int userId)
    {
        var summary = await _reputationService.GetReputationSummaryAsync(userId);
        var violations = await _workOrderRepo.Query()
            .Where(w => w.Type == (int)WorkOrderType.Report && w.AccusedId == userId)
            .ToListAsync();
        var warnings = await _warningRepo.GetRecentByUserIdAsync(userId, 10);
        var warningCount = await _warningRepo.Query().CountAsync(w => w.UserId == userId);

        return new AdminUserReputationDto
        {
            Summary = summary,
            TotalViolations = violations.Count,
            PendingViolations = violations.Count(v => v.Status != "done"),
            WarningCount = warningCount,
            RecentWarnings = warnings.Select(w => new AdminUserWarningDto
            {
                WarningId = w.WarningId,
                Reason = w.Reason,
                CreateTime = w.CreateTime,
                AdminId = w.AdminId,
                AdminName = "管理员"
            }).ToList()
        };
    }

    public async Task<AdminUserDetailDto?> UpdateUserStatusAsync(int userId, UpdateAdminUserStatusDto dto)
    {
        if (!Enum.IsDefined(typeof(AccountStatus), dto.Status))
            throw new ArgumentException("无效的用户状态");

        var user = await _baseUserRepo.GetByIdWithProfileAsync(userId);
        if (user == null) return null;

        user.AccountStatus = dto.Status;
        user.IsBanned = dto.Status == AccountStatus.Banned ? 1 : 0;
        user.BannedUntil = dto.Status == AccountStatus.Banned ? dto.BannedUntil : null;

        _baseUserRepo.Update(user);
        await _baseUserRepo.SaveAsync();

        return await GetUserDetailAsync(userId);
    }

    public async Task<AdminUserWarningDto?> CreateWarningAsync(int userId, CreateUserWarningDto dto, int adminId)
    {
        if (string.IsNullOrWhiteSpace(dto.Reason))
            throw new ArgumentException("警告内容不能为空");

        if (await _baseUserRepo.GetByIdAsync(userId) == null) return null;

        var warning = new UserWarning
        {
            UserId = userId,
            AdminId = adminId,
            Reason = dto.Reason.Trim(),
            CreateTime = DateTime.Now
        };

        await _warningRepo.AddAsync(warning);
        await _warningRepo.SaveAsync();

        return new AdminUserWarningDto
        {
            WarningId = warning.WarningId,
            Reason = warning.Reason,
            CreateTime = warning.CreateTime,
            AdminId = adminId,
            AdminName = "管理员"
        };
    }

    public async Task<AdminUserStatisticsDto> GetStatisticsAsync()
    {
        var today = DateTime.Today;
        var sevenDaysAgo = today.AddDays(-7);

        return new AdminUserStatisticsDto
        {
            TotalUsers = await _baseUserRepo.CountUsersAsync(null, null, null, null),
            NormalUsers = await _baseUserRepo.CountUsersAsync(null, (int)AccountStatus.Normal, null, null),
            MutedUsers = await _baseUserRepo.CountUsersAsync(null, (int)AccountStatus.Muted, null, null),
            PublishRestrictedUsers = await _baseUserRepo.CountUsersAsync(null, (int)AccountStatus.PublishRestricted, null, null),
            BannedUsers = await _baseUserRepo.CountUsersAsync(null, (int)AccountStatus.Banned, null, null),
            NewUsersToday = await _baseUserRepo.CountUsersAsync(null, null, today, null),
            NewUsersThisWeek = await _baseUserRepo.CountUsersAsync(null, null, sevenDaysAgo, null),
            UsersWithProducts = await _productRepo.Query().Select(p => p.UserId).Distinct().CountAsync(),
            TotalOrders = await _purchaseRepo.Query().CountAsync(),
            TotalWorkOrders = await _workOrderRepo.Query().CountAsync(),
            PendingWorkOrders = await _workOrderRepo.Query().CountAsync(w => w.Status != "done"),
            TotalWarnings = await _warningRepo.Query().CountAsync()
        };
    }

    private static AdminUserListItemDto ToListItem(
        BaseUser user,
        IReadOnlyDictionary<int, int> productCounts,
        IReadOnlyDictionary<int, int> orderCounts,
        IReadOnlyDictionary<int, int> warningCounts,
        IReadOnlyDictionary<int, int> violationCounts)
    {
        return new AdminUserListItemDto
        {
            UserId = user.UserId,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            UserName = user.NormUser?.UserName ?? (user.UserType == 1 ? "管理员" : ""),
            UserType = user.UserType,
            AccountStatus = user.AccountStatus,
            IsBanned = user.IsBanned,
            BannedUntil = user.BannedUntil,
            Credit = user.NormUser?.Credit ?? 0,
            RegisterTime = user.RegisterTime,
            ProductCount = productCounts.GetValueOrDefault(user.UserId),
            OrderCount = orderCounts.GetValueOrDefault(user.UserId),
            WarningCount = warningCounts.GetValueOrDefault(user.UserId),
            ViolationCount = violationCounts.GetValueOrDefault(user.UserId)
        };
    }

    private static AdminWorkOrderDto ToWorkOrderDto(WorkOrder w) => new()
    {
        WorkOrderId = w.WorkOrderId,
        Type = w.Type,
        Reason = w.Reason,
        Info = w.Info,
        Status = w.Status,
        CreateTime = w.CreateTime,
        Response = w.Response,
        ResponseTime = w.ResponseTime,
        InitiatorId = w.InitiatorId,
        InitiatorName = w.Initiator?.UserName ?? "",
        AccusedId = w.AccusedId,
        AccusedName = w.Accused?.UserName,
        ProductId = w.ProductId,
        ProductName = w.Product?.Name,
        AdminId = w.AdminId
    };
}

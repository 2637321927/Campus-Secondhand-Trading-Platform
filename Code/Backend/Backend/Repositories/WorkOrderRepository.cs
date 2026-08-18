using Backend.Data;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories;

public class WorkOrderRepository : IWorkOrderRepository
{
    private readonly AppDbContext _context;
    public WorkOrderRepository(AppDbContext context) => _context = context;

    public IQueryable<WorkOrder> Query()
        => _context.WorkOrders.AsQueryable();

    public async Task<WorkOrder?> GetByIdAsync(long workOrderId)
        => await _context.WorkOrders.FindAsync(workOrderId);

    public async Task<List<WorkOrder>> GetAllAsync()
        => await _context.WorkOrders.ToListAsync();

    public async Task<List<WorkOrder>> GetByInitiatorIdAsync(int userId)
        => await _context.WorkOrders.Where(w => w.InitiatorId == userId).ToListAsync();

    public async Task<List<WorkOrder>> GetByStatusAsync(string status)
        => await _context.WorkOrders.Where(w => w.Status == status).ToListAsync();

    public async Task<List<WorkOrder>> GetRelatedByUserIdAsync(
        int userId,
        int type,
        string? status,
        bool includeAccused)
    {
        var query = _context.WorkOrders.Where(w => w.Type == type);

        if (includeAccused)
            query = query.Where(w => w.InitiatorId == userId || w.AccusedId == userId);
        else
            query = query.Where(w => w.InitiatorId == userId);

        if (!string.IsNullOrEmpty(status))
            query = query.Where(w => w.Status == status);

        return await query
            .Include(w => w.Initiator)
            .Include(w => w.Accused)
            .Include(w => w.Product)
            .Include(w => w.Admin)
            .OrderByDescending(w => w.CreateTime)
            .ToListAsync();
    }

    public async Task<(List<WorkOrder> Items, int Total)> GetAdminPageAsync(
        int type,
        string? keyword,
        string? status,
        string? targetType,
        int page,
        int pageSize)
    {
        var query = _context.WorkOrders.Where(w => w.Type == type);

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var kw = keyword.Trim();
            query = query.Where(w =>
                w.Reason.Contains(kw) ||
                (w.Info != null && w.Info.Contains(kw)) ||
                (w.Initiator != null && w.Initiator.UserName.Contains(kw)) ||
                (w.Accused != null && w.Accused.UserName.Contains(kw)) ||
                (w.Product != null && w.Product.Name.Contains(kw)));
        }

        if (!string.IsNullOrEmpty(status))
            query = query.Where(w => w.Status == status);

        if (!string.IsNullOrEmpty(targetType))
            query = query.Where(w => w.TargetType == targetType);

        var total = await query.CountAsync();
        var items = await query
            .Include(w => w.Initiator)
            .Include(w => w.Accused)
            .Include(w => w.Product)
            .Include(w => w.Admin)
            .Include(w => w.AppealAgainst)
            .OrderByDescending(w => w.CreateTime)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }

    public async Task<WorkOrder?> GetDetailAsync(long workOrderId)
        => await _context.WorkOrders
            .Include(w => w.Initiator)
            .Include(w => w.Accused)
            .Include(w => w.Product)
            .Include(w => w.Admin)
            .Include(w => w.AppealAgainst)
            .ThenInclude(w => w!.Initiator)
            .FirstOrDefaultAsync(w => w.WorkOrderId == workOrderId);

    public async Task AddAsync(WorkOrder workOrder)
        => await _context.WorkOrders.AddAsync(workOrder);

    public void Update(WorkOrder workOrder)
        => _context.WorkOrders.Update(workOrder);

    public void Delete(WorkOrder workOrder)
        => _context.WorkOrders.Remove(workOrder);

    public async Task SaveAsync()
        => await _context.SaveChangesAsync();
}

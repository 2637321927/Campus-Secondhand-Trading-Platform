using Backend.Data;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories;

public class WorkOrderRepository : IWorkOrderRepository
{
    private readonly AppDbContext _context;
    public WorkOrderRepository(AppDbContext context) => _context = context;

    public async Task<WorkOrder?> GetByIdAsync(long workOrderId)
        => await _context.WorkOrders.FindAsync(workOrderId);

    public async Task<List<WorkOrder>> GetAllAsync()
        => await _context.WorkOrders.ToListAsync();

    public async Task<List<WorkOrder>> GetByInitiatorIdAsync(int userId)
        => await _context.WorkOrders.Where(w => w.InitiatorId == userId).ToListAsync();

    public async Task<List<WorkOrder>> GetByStatusAsync(string status)
        => await _context.WorkOrders.Where(w => w.Status == status).ToListAsync();

    public async Task AddAsync(WorkOrder workOrder)
        => await _context.WorkOrders.AddAsync(workOrder);

    public void Update(WorkOrder workOrder)
        => _context.WorkOrders.Update(workOrder);

    public void Delete(WorkOrder workOrder)
        => _context.WorkOrders.Remove(workOrder);

    public async Task SaveAsync()
        => await _context.SaveChangesAsync();
}

using Backend.Data;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories;

public class WorkOrderTimelineRepository : IWorkOrderTimelineRepository
{
    private readonly AppDbContext _context;

    public WorkOrderTimelineRepository(AppDbContext context) => _context = context;

    public IQueryable<WorkOrderTimeline> Query()
        => _context.WorkOrderTimelines.AsQueryable();

    public async Task<List<WorkOrderTimeline>> GetByWorkOrderIdAsync(long workOrderId)
        => await _context.WorkOrderTimelines
            .Where(t => t.WorkOrderId == workOrderId)
            .Include(t => t.Admin)
            .OrderByDescending(t => t.CreateTime)
            .ToListAsync();

    public async Task AddAsync(WorkOrderTimeline timeline)
        => await _context.WorkOrderTimelines.AddAsync(timeline);

    public async Task SaveAsync()
        => await _context.SaveChangesAsync();
}

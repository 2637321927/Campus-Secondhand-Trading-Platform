using Backend.Data;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories;

public class OrderTimelineRepository : IOrderTimelineRepository
{
    private readonly AppDbContext _context;
    public OrderTimelineRepository(AppDbContext context) => _context = context;

    public async Task<List<OrderTimeline>> GetByPurchaseIdAsync(long purchaseId)
        => await _context.OrderTimelines
            .Where(t => t.PurchaseId == purchaseId)
            .OrderBy(t => t.ChangeTime)
            .ToListAsync();

    public async Task AddAsync(OrderTimeline timeline)
        => await _context.OrderTimelines.AddAsync(timeline);

    public async Task SaveAsync()
        => await _context.SaveChangesAsync();
}

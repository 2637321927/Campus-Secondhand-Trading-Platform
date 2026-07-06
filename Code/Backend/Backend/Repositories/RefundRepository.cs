using Backend.Data;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories;

public class RefundRepository : IRefundRepository
{
    private readonly AppDbContext _context;
    public RefundRepository(AppDbContext context) => _context = context;

    public async Task<Refund?> GetByIdAsync(long refundId)
        => await _context.Refunds.FindAsync(refundId);

    public async Task<List<Refund>> GetAllAsync()
        => await _context.Refunds.ToListAsync();

    public async Task<Refund?> GetByPurchaseIdAsync(long purchaseId)
        => await _context.Refunds.FirstOrDefaultAsync(r => r.PurchaseId == purchaseId);

    public async Task AddAsync(Refund refund)
        => await _context.Refunds.AddAsync(refund);

    public void Update(Refund refund)
        => _context.Refunds.Update(refund);

    public void Delete(Refund refund)
        => _context.Refunds.Remove(refund);

    public async Task SaveAsync()
        => await _context.SaveChangesAsync();
}

using Backend.Data;
using Backend.Models;
using Backend.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories;

public class PaymentRepository : IPaymentRepository
{
    private readonly AppDbContext _context;
    public PaymentRepository(AppDbContext context) => _context = context;

    public async Task<Payment?> GetByIdAsync(long paymentId)
        => await _context.Payments
            .Include(p => p.Purchase)
            .FirstOrDefaultAsync(p => p.PaymentId == paymentId);

    public async Task<List<Payment>> GetByPurchaseIdAsync(long purchaseId)
        => await _context.Payments
            .Where(p => p.PurchaseId == purchaseId)
            .OrderByDescending(p => p.CreateTime)
            .ToListAsync();

    public async Task<Payment?> GetPendingByPurchaseIdAsync(long purchaseId)
        => await _context.Payments
            .Where(p => p.PurchaseId == purchaseId && p.Status == PaymentStatus.Pending)
            .FirstOrDefaultAsync();

    public async Task AddAsync(Payment payment)
        => await _context.Payments.AddAsync(payment);

    public void Update(Payment payment)
        => _context.Payments.Update(payment);

    public async Task SaveAsync()
        => await _context.SaveChangesAsync();
}

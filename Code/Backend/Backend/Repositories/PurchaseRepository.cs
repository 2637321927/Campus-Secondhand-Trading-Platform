using Backend.Data;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories;

public class PurchaseRepository : IPurchaseRepository
{
    private readonly AppDbContext _context;
    public PurchaseRepository(AppDbContext context) => _context = context;

    public async Task<Purchase?> GetByIdAsync(long purchaseId)
        => await _context.Purchases
            .Include(p => p.Product)
            .Include(p => p.Buyer)
            .FirstOrDefaultAsync(p => p.PurchaseId == purchaseId);

    public async Task<List<Purchase>> GetAllAsync()
        => await _context.Purchases.Include(p => p.Product).ToListAsync();

    public async Task<List<Purchase>> GetByBuyerIdAsync(int buyerId)
        => await _context.Purchases.Where(p => p.BuyerId == buyerId).Include(p => p.Product).ToListAsync();

    public async Task<List<Purchase>> GetByStatusAsync(string status)
        => await _context.Purchases.Where(p => p.Status == status).Include(p => p.Product).ToListAsync();

    public async Task AddAsync(Purchase purchase)
        => await _context.Purchases.AddAsync(purchase);

    public void Update(Purchase purchase)
        => _context.Purchases.Update(purchase);

    public void Delete(Purchase purchase)
        => _context.Purchases.Remove(purchase);

    public async Task SaveAsync()
        => await _context.SaveChangesAsync();
}

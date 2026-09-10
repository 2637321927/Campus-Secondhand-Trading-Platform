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
                .ThenInclude(p => p!.Images)
            .Include(p => p.Product!.Seller)
            .Include(p => p.Buyer)
            .Include(p => p.Address)
            .Include(p => p.Review)
            .Include(p => p.Timelines)
            .FirstOrDefaultAsync(p => p.PurchaseId == purchaseId);

    public async Task<List<Purchase>> GetAllAsync()
        => await _context.Purchases
            .Include(p => p.Product)
            .Include(p => p.Buyer)
            .ToListAsync();

    public async Task<List<Purchase>> GetByBuyerIdAsync(int buyerId)
        => await _context.Purchases
            .Where(p => p.BuyerId == buyerId)
            .Include(p => p.Product)
            .ThenInclude(p => p!.Seller)
            .Include(p => p.Product)
            .ThenInclude(p => p!.Images)
            .Include(p => p.Buyer)
            .OrderByDescending(p => p.CreateTime)
            .ToListAsync();

    public async Task<List<Purchase>> GetBySellerUserIdAsync(int sellerUserId)
        => await _context.Purchases
            .Where(p => p.Product!.UserId == sellerUserId)
            .Include(p => p.Product)
            .ThenInclude(p => p!.Seller)
            .Include(p => p.Product)
            .ThenInclude(p => p!.Images)
            .Include(p => p.Buyer)
            .OrderByDescending(p => p.CreateTime)
            .ToListAsync();

    public async Task<List<Purchase>> GetBySellerIdAsync(int sellerId)
        => await _context.Purchases
            .Where(p => p.Product != null && p.Product.UserId == sellerId)
            .Include(p => p.Product)
                .ThenInclude(p => p!.Images)
            .Include(p => p.Buyer)
            .Include(p => p.Review)
            .OrderByDescending(p => p.CreateTime)
            .ToListAsync();

    public async Task<List<Purchase>> GetByProductIdAsync(long productId)
        => await _context.Purchases
            .Where(p => p.ProductId == productId)
            .Include(p => p.Buyer)
            .ToListAsync();

    public async Task<List<Purchase>> GetByStatusAsync(string status)
        => await _context.Purchases
            .Where(p => p.Status == status)
            .Include(p => p.Product)
            .Include(p => p.Buyer)
            .ToListAsync();

    public async Task<(List<Purchase> Items, int Total)> GetAdminPageAsync(
        long? orderId,
        string? status,
        DateTime? startDate,
        DateTime? endDate,
        int page,
        int pageSize)
    {
        var query = _context.Purchases
            .Include(p => p.Product)
                .ThenInclude(p => p!.Images)
            .Include(p => p.Product)
                .ThenInclude(p => p!.Seller)
            .Include(p => p.Buyer)
            .AsQueryable();

        if (orderId.HasValue)
            query = query.Where(p => p.PurchaseId == orderId.Value);

        if (!string.IsNullOrEmpty(status))
            query = query.Where(p => p.Status == status);

        if (startDate.HasValue)
            query = query.Where(p => p.CreateTime >= startDate.Value);

        if (endDate.HasValue)
        {
            var end = endDate.Value.Date.AddDays(1);
            query = query.Where(p => p.CreateTime < end);
        }

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(p => p.CreateTime)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }

    public IQueryable<Purchase> Query()
        => _context.Purchases.AsQueryable();

    public async Task AddAsync(Purchase purchase)
        => await _context.Purchases.AddAsync(purchase);

    public void Update(Purchase purchase)
        => _context.Purchases.Update(purchase);

    public void Delete(Purchase purchase)
        => _context.Purchases.Remove(purchase);

    public async Task SaveAsync()
        => await _context.SaveChangesAsync();
}

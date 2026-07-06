using Backend.Data;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories;

public class ProdImageRepository : IProdImageRepository
{
    private readonly AppDbContext _context;
    public ProdImageRepository(AppDbContext context) => _context = context;

    public async Task<ProdImage?> GetByIdAsync(long imgId)
        => await _context.ProdImages.FindAsync(imgId);

    public async Task<List<ProdImage>> GetByProductIdAsync(long productId)
        => await _context.ProdImages.Where(p => p.ProductId == productId).ToListAsync();

    public async Task AddAsync(ProdImage image)
        => await _context.ProdImages.AddAsync(image);

    public void Update(ProdImage image)
        => _context.ProdImages.Update(image);

    public void Delete(ProdImage image)
        => _context.ProdImages.Remove(image);

    public async Task SaveAsync()
        => await _context.SaveChangesAsync();
}

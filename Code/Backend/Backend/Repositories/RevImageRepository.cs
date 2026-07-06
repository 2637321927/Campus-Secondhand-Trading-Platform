using Backend.Data;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories;

public class RevImageRepository : IRevImageRepository
{
    private readonly AppDbContext _context;
    public RevImageRepository(AppDbContext context) => _context = context;

    public async Task<RevImage?> GetByIdAsync(long imgId)
        => await _context.RevImages.FindAsync(imgId);

    public async Task<List<RevImage>> GetByReviewIdAsync(int reviewId)
        => await _context.RevImages.Where(r => r.ReviewId == reviewId).ToListAsync();

    public async Task AddAsync(RevImage image)
        => await _context.RevImages.AddAsync(image);

    public void Update(RevImage image)
        => _context.RevImages.Update(image);

    public void Delete(RevImage image)
        => _context.RevImages.Remove(image);

    public async Task SaveAsync()
        => await _context.SaveChangesAsync();
}

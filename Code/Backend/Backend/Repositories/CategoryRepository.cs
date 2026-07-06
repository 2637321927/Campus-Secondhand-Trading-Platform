using Backend.Data;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly AppDbContext _context;
    public CategoryRepository(AppDbContext context) => _context = context;

    public async Task<Category?> GetByIdAsync(long categoryId)
        => await _context.Categories.FindAsync(categoryId);

    public async Task<List<Category>> GetAllAsync()
        => await _context.Categories.ToListAsync();

    public async Task<List<Category>> GetChildrenAsync(long parentId)
        => await _context.Categories.Where(c => c.ParentId == parentId).ToListAsync();

    public async Task<List<Category>> GetRootCategoriesAsync()
        => await _context.Categories.Where(c => c.ParentId == null).ToListAsync();

    public async Task AddAsync(Category category)
        => await _context.Categories.AddAsync(category);

    public void Update(Category category)
        => _context.Categories.Update(category);

    public void Delete(Category category)
        => _context.Categories.Remove(category);

    public async Task SaveAsync()
        => await _context.SaveChangesAsync();
}

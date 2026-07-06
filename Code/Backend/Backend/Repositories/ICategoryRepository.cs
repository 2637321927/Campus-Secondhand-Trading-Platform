using Backend.Models;

namespace Backend.Repositories;

public interface ICategoryRepository
{
    Task<Category?> GetByIdAsync(long categoryId);
    Task<List<Category>> GetAllAsync();
    Task<List<Category>> GetChildrenAsync(long parentId);
    Task<List<Category>> GetRootCategoriesAsync();
    Task AddAsync(Category category);
    void Update(Category category);
    void Delete(Category category);
    Task SaveAsync();
}

using Backend.Models;

namespace Backend.Repositories;

public interface ICategoryRepository
{
    Task<Category?> GetByIdAsync(int categoryId);
    Task<List<Category>> GetAllAsync();
    Task<List<Category>> GetChildrenAsync(int parentId);
    Task<List<Category>> GetRootCategoriesAsync();
    Task AddAsync(Category category);
    void Update(Category category);
    void Delete(Category category);
    Task SaveAsync();
}

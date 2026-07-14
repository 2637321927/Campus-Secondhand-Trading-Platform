using Backend.Dtos.Category;
using Backend.Dtos.Product;
using Backend.Models;
using Backend.Repositories;

namespace Backend.Services;
public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepo;
    private readonly IProductRepository _productRepo;

    public CategoryService(ICategoryRepository categoryRepo, IProductRepository productRepo)
    {
        _categoryRepo = categoryRepo;
        _productRepo = productRepo;
    }
    public async Task<CategoryDto?> GetByIdAsync(int categoryId)
    {
        var category = await _categoryRepo.GetByIdAsync(categoryId);
        if (category == null) return null;
        string? parentName = null;
        if (category.ParentId != null)
        {
            var parent = await _categoryRepo.GetByIdAsync(category.ParentId.Value);
            parentName = parent?.CategoryName;
        }
        return new CategoryDto
        {
            CategoryId = category.CategoryId,
            CategoryName = category.CategoryName,
            ParentId = category.ParentId,
            ParentName = parentName
        };
    }
    public async Task<List<CategoryDto>> GetAllAsync()
    {
        var categories = await _categoryRepo.GetAllAsync();
        //建ID→Name字典，减少查数据库的次数
        var nameDict = categories.ToDictionary(c => c.CategoryId, c => c.CategoryName);
        return categories.Select(c => new CategoryDto
        {
            CategoryId = c.CategoryId,
            CategoryName = c.CategoryName,
            ParentId = c.ParentId,
            ParentName = c.ParentId != null ? nameDict.GetValueOrDefault(c.ParentId.Value) : null
        }).ToList();
    }
    
    public async Task<List<CategoryDto>> GetChildrenAsync(int parentId)
    {
        var children = await _categoryRepo.GetChildrenAsync(parentId);
        var parent = await _categoryRepo.GetByIdAsync(parentId);
        string? parentName = parent?.CategoryName;
        return children.Select(c => new CategoryDto
        {
            CategoryId = c.CategoryId,
            CategoryName = c.CategoryName,
            ParentId = c.ParentId,
            ParentName = parentName
        }).ToList();
    }

    public async Task<bool> CreateCategoryAsync(CreateCategoryDto dto)
    {
        var category = new Category
        {
            CategoryName = dto.Name,
            ParentId = dto.ParentId
        };

        await _categoryRepo.AddAsync(category);
        await _categoryRepo.SaveAsync();
        return true;
    }

    public async Task<bool> DeleteCategoryAsync(int categoryId)
    {
        var category = await _categoryRepo.GetByIdAsync(categoryId);
        if (category == null) return false;

        _categoryRepo.Delete(category);
        await _categoryRepo.SaveAsync();
        return true;
    }

    public async Task<List<ProductDto>> GetProductsByCategoryAsync(int categoryId)
    {
        var products = await _productRepo.GetByCategoryAsync(categoryId);
        return products.Select(p => new ProductDto
        {
            ProductId = p.ProductId,
            Name = p.Name,
            Price = p.Price,
            Info = p.Info,
            Status = p.Status,
            ReleaseDate = p.ReleaseDate,
            UserId = p.UserId,
            CategoryId = p.CategoryId,
            CategoryName = p.Category?.CategoryName,
            Images = p.Images?.Select(i => new ProductImageDto
            {
                ImgFileId = i.ImgFileId,
                ImgIndex = i.ImgIndex
            }).ToList() ?? new()
        }).ToList();
    }
}
using Backend.Dtos.Category;
using Backend.Dtos.Product;

namespace Backend.Services;

public interface ICategoryService
{
    /// <summary>
    /// 根据 Id 获取分类信息
    /// </summary>
    Task<CategoryDto?> GetByIdAsync(long categoryId);

    /// <summary>
    /// 获取所有分类
    /// </summary>
    Task<List<CategoryDto>> GetAllAsync();

    /// <summary>
    /// 获取子分类
    /// </summary>
    Task<List<CategoryDto>> GetChildrenAsync(long parentId);

    /// <summary>
    /// 创建分类（目前没有考虑用户增加分类，仅调试用）
    /// </summary>
    Task<bool> CreateCategoryAsync(CreateCategoryDto dto);

    /// <summary>
    /// 删除分类（同上，仅调试用）
    /// </summary>
    Task<bool> DeleteCategoryAsync(long categoryId);

    /// <summary>
    /// 获取某分类下的商品列表（占位，后续切到 ProductService，也可以不切，有待讨论）
    /// </summary>
    Task<List<ProductDto>> GetProductsByCategoryAsync(long categoryId);
}
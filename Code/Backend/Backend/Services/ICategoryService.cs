using Backend.Dtos.Category;
using Backend.Dtos.Product;

namespace Backend.Services;

public interface ICategoryService
{
    Task<CategoryDto?> GetByIdAsync(int categoryId);
    Task<List<CategoryDto>> GetAllAsync();
    Task<List<CategoryDto>> GetChildrenAsync(int parentId);
    //TODO：以下考虑正式版本删除
   // Task<List<CategoryDto>> GetRootCategoriesAsync(); 获取根目录，可以通过GetAll加过滤获得
    Task<bool> CreateCategoryAsync(CreateCategoryDto dto); //目前没有考虑用户增加分类，仅调试用
    Task<bool> DeleteCategoryAsync(int categoryId);  //同理
    /// <summary>
    /// 查某分类下的商品列表（占位，后续切到 ProductService）
    /// </summary>
    Task<List<ProductDto>> GetProductsByCategoryAsync(int categoryId);
}
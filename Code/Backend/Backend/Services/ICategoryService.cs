using Backend.Dtos.Category;
using Backend.Dtos.Product;

namespace Backend.Services;

public interface ICategoryService
{
    ///<summary>
    ///根据Id获取分类信息
    ///</summary>
    Task<CategoryDto?> GetByIdAsync(int categoryId);
    ///<summary>
    ///获取所有分类
    ///</summary>
    Task<List<CategoryDto>> GetAllAsync();
    ///<summary>
    ///获取子分类
    ///</summary>
    Task<List<CategoryDto>> GetChildrenAsync(int parentId);
    //TODO：以下考虑正式版本删除
   // Task<List<CategoryDto>> GetRootCategoriesAsync(); 获取根目录，可以通过GetAll加过滤获得
    ///<summary>
    ///创建分类
    ///</summary>
    Task<bool> CreateCategoryAsync(CreateCategoryDto dto); //目前没有考虑用户增加分类，仅调试用
    ///<summary>
    ///删除分类
    ///</summary>
    Task<bool> DeleteCategoryAsync(int categoryId);  //同理
    ///<summary>
    ///获取某分类下的商品列表
    ///</summary>
    Task<List<ProductDto>> GetProductsByCategoryAsync(int categoryId);//查某分类下的商品列表（占位，后续切到 ProductService,也可以不切，有待讨论）
}
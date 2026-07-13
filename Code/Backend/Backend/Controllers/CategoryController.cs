using Backend.Dtos.Category;
using Backend.Dtos.Product;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

///<summary>
///分类模块
///</summary>
[ApiController]
[Route("api/categories")]
public class CategoryController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoryController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    ///<summary>
    ///获取商品分类列表
    ///</summary>
    [HttpGet]
    public async Task<ActionResult<List<CategoryDto>>> GetAll()
    {
        var categories = await _categoryService.GetAllAsync();
        return Ok(categories);
    }

    ///<summary>
    ///获取单个分类信息
    ///</summary>
    [HttpGet("{categoryId}")]
    public async Task<ActionResult<CategoryDto>> GetById(int categoryId)
    {
        var category = await _categoryService.GetByIdAsync(categoryId);
        if (category == null) return NotFound();
        return Ok(category);
    }

    ///<summary>
    ///获取某个分类下的商品列表
    ///</summary>
    [HttpGet("{categoryId}/products")]
    public async Task<ActionResult<List<ProductDto>>> GetProducts(int categoryId)
    {
        var category = await _categoryService.GetByIdAsync(categoryId);
        if (category == null) return NotFound(new { error = "分类不存在" });

        var products = await _categoryService.GetProductsByCategoryAsync(categoryId);
        return Ok(products);
    }

    ///<summary>
    ///获取某分类下的子分类列表
    ///</summary>
    [HttpGet("{categoryId}/children")]
    public async Task<ActionResult<List<CategoryDto>>> GetChildren(int categoryId)
    {
        var category = await _categoryService.GetByIdAsync(categoryId);
        if (category == null) return NotFound(new { error = "分类不存在" });

        var children = await _categoryService.GetChildrenAsync(categoryId);
        return Ok(children);
    }

    ///<summary>
    ///创建分类（调试用）
    ///</summary>
    [HttpPost]
    public async Task<ActionResult> Create([FromBody] CreateCategoryDto dto)
    {
        await _categoryService.CreateCategoryAsync(dto);
        return Created();
    }

    ///<summary>
    ///删除分类（调试用）
    ///</summary>
    [HttpDelete("{categoryId}")]
    public async Task<ActionResult> Delete(int categoryId)
    {
        var result = await _categoryService.DeleteCategoryAsync(categoryId);
        if (!result) return NotFound();
        return NoContent();
    }
}
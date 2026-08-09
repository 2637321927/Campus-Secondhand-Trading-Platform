using Backend.Dtos.Product;
using Backend.Dtos.User;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

/// <summary>
/// 用户信息查询与个人中心
/// </summary>
[ApiController]
[Route("api/users")]
public class UserController : ControllerBase
{
    private readonly IBaseUserService _userService;
    private readonly IProductService _productService;
    private readonly ISearchService _searchService;
    private readonly IProductCommentService _commentService;

    public UserController(IBaseUserService userService, IProductService productService, ISearchService searchService, IProductCommentService commentService)
    {
        _userService = userService;
        _productService = productService;
        _searchService = searchService;
        _commentService = commentService;
    }

    /// <summary>
    /// 根据 ID 获取用户公开信息
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<UserDto>> GetById(int id)
    {
        var user = await _userService.GetByIdAsync(id);
        if (user == null) return NotFound();
        return Ok(user);
    }

    /// <summary>
    /// 查看某用户已发布商品
    /// </summary>
    [HttpGet("{userId:int}/products")]
    public async Task<ActionResult<List<ProductDto>>> GetUserProducts(int userId)
    {
        if (userId <= 0)
            return BadRequest(new { error = "userId must be greater than zero." });

        var products = await _productService.GetProductsByUserIdAsync(userId);
        return Ok(products);
    }

    /// <summary>
    /// 查看某用户已卖出商品
    /// </summary>
    [HttpGet("{userId:int}/sold-products")]
    public async Task<ActionResult<List<ProductDto>>> GetUserSoldProducts(int userId)
    {
        if (userId <= 0)
            return BadRequest(new { error = "userId must be greater than zero." });

        var products = await _productService.GetSoldProductsByUserIdAsync(userId);
        return Ok(products);
    }

    /// <summary>
    /// 在某用户主页内搜索其发布的商品
    /// </summary>
    [HttpGet("{userId:int}/product-search")]
    public async Task<ActionResult<SearchResultDto>> SearchUserProducts(
        int userId,
        [FromQuery] string? keyword = null,
        [FromQuery] string? searchId = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? sortBy = null)
    {
        if (userId <= 0)
            return BadRequest(new { error = "userId must be greater than zero." });

        if (string.IsNullOrWhiteSpace(keyword) && string.IsNullOrWhiteSpace(searchId))
            return BadRequest(new { message = "keyword 和 searchId 不能同时为空" });

        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 20;
        if (pageSize > 50) pageSize = 50;

        var request = new SearchRequestDto
        {
            SearchId = searchId,
            Keyword = (keyword ?? "").Trim(),
            Page = page,
            PageSize = pageSize,
            SortBy = sortBy,
            UserId = userId
        };

        var result = await _searchService.SearchProductAsync(request);
        return Ok(result);
    }

    /// <summary>
    /// 查看某用户收到的评价（其商品被他人评论）
    /// </summary>
    [HttpGet("{userId:int}/reviews/received")]
    public async Task<ActionResult<List<UserCommentDto>>> GetReceivedReviews(int userId)
    {
        if (userId <= 0)
            return BadRequest(new { error = "userId must be greater than zero." });

        var comments = await _commentService.GetUserReceivedCommentsAsync(userId);
        return Ok(comments);
    }

    /// <summary>
    /// 查看某用户发出的评价（其对他人商品的评论）
    /// </summary>
    [HttpGet("{userId:int}/reviews/given")]
    public async Task<ActionResult<List<UserCommentDto>>> GetGivenReviews(int userId)
    {
        if (userId <= 0)
            return BadRequest(new { error = "userId must be greater than zero." });

        var comments = await _commentService.GetUserGivenCommentsAsync(userId);
        return Ok(comments);
    }

    /// <summary>
    /// 获取个人中心资料
    /// </summary>
    [Authorize]
    [HttpGet("me/profile")]
    public async Task<ActionResult<UserProfileDto>> GetMyProfile()
    {
        var userId = int.Parse(User.FindFirst("userId")!.Value);
        var profile = await _userService.GetProfileAsync(userId);
        if (profile == null) return NotFound(new { error = "用户不存在" });
        return Ok(profile);
    }

    /// <summary>
    /// 修改个人中心资料（支持部分更新，仅传入需要修改的字段）
    /// </summary>
    [Authorize]
    [HttpPut("me/profile")]
    public async Task<ActionResult<UserProfileDto>> UpdateMyProfile([FromBody] UpdateProfileDto dto)
    {
        try
        {
            var userId = int.Parse(User.FindFirst("userId")!.Value);
            var profile = await _userService.UpdateProfileAsync(userId, dto);
            if (profile == null) return NotFound(new { error = "用户不存在" });
            return Ok(profile);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// 上传/更换当前用户头像（multipart/form-data）
    /// </summary>
    [Authorize]
    [HttpPost("me/avatar")]
    public async Task<ActionResult<AvatarUploadResponseDto>> UploadAvatar(IFormFile file)
    {
        try
        {
            var userId = int.Parse(User.FindFirst("userId")!.Value);
            var result = await _userService.UploadAvatarAsync(userId, file);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}

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
    private readonly IPurchaseService _purchaseService;
    private readonly IProductViewService _viewService;
    private readonly IAddressService _addressService;

    public UserController(IBaseUserService userService, IProductService productService, ISearchService searchService, IProductCommentService commentService, IPurchaseService purchaseService, IProductViewService viewService, IAddressService addressService)
    {
        _userService = userService;
        _productService = productService;
        _searchService = searchService;
        _commentService = commentService;
        _purchaseService = purchaseService;
        _viewService = viewService;
        _addressService = addressService;
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

    /// <summary>
    /// 当前用户"我发布"的商品列表
    /// </summary>
    [Authorize]
    [HttpGet("me/published-products")]
    public async Task<ActionResult<List<ProductDto>>> GetMyPublishedProducts()
    {
        var userId = int.Parse(User.FindFirst("userId")!.Value);
        var products = await _productService.GetProductsByUserIdAsync(userId);
        return Ok(products);
    }

    /// <summary>
    /// 当前用户"我卖出"的订单列表
    /// </summary>
    [Authorize]
    [HttpGet("me/sold-orders")]
    public async Task<ActionResult<List<PurchaseDto>>> GetMySoldOrders()
    {
        var userId = int.Parse(User.FindFirst("userId")!.Value);
        var orders = await _purchaseService.GetMySoldOrdersAsync(userId);
        return Ok(orders);
    }

    /// <summary>
    /// 当前用户"我购买"的订单列表
    /// </summary>
    [Authorize]
    [HttpGet("me/purchase-orders")]
    public async Task<ActionResult<List<PurchaseDto>>> GetMyPurchaseOrders()
    {
        var userId = int.Parse(User.FindFirst("userId")!.Value);
        var orders = await _purchaseService.GetMyPurchasesAsync(userId);
        return Ok(orders);
    }

    /// <summary>
    /// 获取当前用户浏览历史
    /// </summary>
    [Authorize]
    [HttpGet("me/browse-history")]
    public async Task<ActionResult<List<BrowseHistoryDto>>> GetMyBrowseHistory()
    {
        var userId = int.Parse(User.FindFirst("userId")!.Value);
        var history = await _viewService.GetBrowseHistoryAsync(userId);
        return Ok(history);
    }

    /// <summary>
    /// 删除某条商品浏览历史
    /// </summary>
    [Authorize]
    [HttpDelete("me/browse-history/{productId:int}")]
    public async Task<IActionResult> DeleteBrowseHistoryItem(long productId)
    {
        var userId = int.Parse(User.FindFirst("userId")!.Value);
        await _viewService.DeleteBrowseHistoryItemAsync(userId, productId);
        return NoContent();
    }

    /// <summary>
    /// 清空当前用户浏览历史
    /// </summary>
    [Authorize]
    [HttpDelete("me/browse-history")]
    public async Task<IActionResult> ClearMyBrowseHistory()
    {
        var userId = int.Parse(User.FindFirst("userId")!.Value);
        await _viewService.ClearBrowseHistoryAsync(userId);
        return NoContent();
    }

    /// <summary>
    /// 获取当前用户地址列表
    /// </summary>
    [Authorize]
    [HttpGet("me/addresses")]
    public async Task<ActionResult<List<AddressDto>>> GetMyAddresses()
    {
        var userId = int.Parse(User.FindFirst("userId")!.Value);
        var addresses = await _addressService.GetMyAddressesAsync(userId);
        return Ok(addresses);
    }

    /// <summary>
    /// 新增收货或交易地址
    /// </summary>
    [Authorize]
    [HttpPost("me/addresses")]
    public async Task<ActionResult<AddressDto>> CreateAddress([FromBody] CreateAddressDto dto)
    {
        try
        {
            var userId = int.Parse(User.FindFirst("userId")!.Value);
            var address = await _addressService.CreateAddressAsync(userId, dto);
            return Ok(address);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// 获取单个地址详情
    /// </summary>
    [Authorize]
    [HttpGet("me/addresses/{addressId:int}")]
    public async Task<ActionResult<AddressDto>> GetAddress(int addressId)
    {
        var userId = int.Parse(User.FindFirst("userId")!.Value);
        var address = await _addressService.GetAddressByIdAsync(userId, addressId);
        if (address == null) return NotFound(new { error = "地址不存在或无权访问" });
        return Ok(address);
    }
}

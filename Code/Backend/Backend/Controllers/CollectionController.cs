using Backend.Dtos.Collection;
using Backend.Dtos.Product;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/collections")]
public class CollectionController : ControllerBase
{
    private readonly ICollectionService _collectionService;

    public CollectionController(ICollectionService collectionService)
    {
        _collectionService = collectionService;
    }

    ///<summary>
    ///收藏/取消收藏（Toggle）
    ///</summary>
    [Authorize]
    [HttpPost("{productId}")]
    public async Task<ActionResult<CollectionStatusResponse>> Toggle(long productId)
    {
        var userId = int.Parse(User.FindFirst("userId")!.Value);
        var isCollected = await _collectionService.ToggleAsync(productId, userId);
        return Ok(new CollectionStatusResponse { IsCollected = isCollected });
    }
    ///<summary>
    ///批量取消收藏
    ///</summary>
    [Authorize]
    [HttpDelete]
    public async Task<ActionResult> BatchDelete([FromBody] List<long> productIds)
    {
        var userId = int.Parse(User.FindFirst("userId")!.Value);
        var deleted = await _collectionService.BatchDeleteAsync(userId, productIds);
        return Ok(new { deleted });
    }

    ///<summary>
    ///查询某商品是否已收藏
    ///</summary>
    [Authorize]
    [HttpGet("{productId}")]
    public async Task<ActionResult<CollectionStatusResponse>> GetStatus(long productId)
    {
        var userId = int.Parse(User.FindFirst("userId")!.Value);
        var isCollected = await _collectionService.IsCollectedAsync(productId, userId);

        return Ok(new CollectionStatusResponse { IsCollected = isCollected });
    }

    ///<summary>
    ///获取我的收藏列表
    ///</summary>
    [Authorize]
    [HttpGet]
    public async Task<ActionResult<List<ProductCardDto>>> GetMyCollections()
    {
        var userId = int.Parse(User.FindFirst("userId")!.Value);
        var items = await _collectionService.GetMyCollectionsAsync(userId);
        return Ok(items);
    }

    ///<summary>
    ///在收藏列表内搜索商品
    ///</summary>
    [Authorize]
    [HttpGet("search")]
    public async Task<ActionResult<List<ProductCardDto>>> Search([FromQuery] string keyword)
    {
        var userId = int.Parse(User.FindFirst("userId")!.Value);
        var items = await _collectionService.SearchAsync(userId, keyword);
        return Ok(items);
    }

    ///<summary>
    ///获取收藏总数（按用户ID，就是我的收藏总数）
    ///</summary>
    [Authorize]
    [HttpGet("count")]
    public async Task<ActionResult> GetCount()
    {
        var userId = int.Parse(User.FindFirst("userId")!.Value);
        var count = await _collectionService.GetCollectionCountAsync(userId);

        return Ok(new { count });
    }
}

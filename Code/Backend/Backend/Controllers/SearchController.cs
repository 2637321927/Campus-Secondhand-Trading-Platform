using Backend.Dtos.Product;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/products")]
public class SearchController : ControllerBase
{
    private readonly ISearchService _searchService;

    public SearchController(ISearchService searchService)
    {
        _searchService = searchService;
    }

    /// <summary>
    /// 商品搜索
    /// </summary>
    /// <param name="keyword">搜索关键词（首次搜索必填）</param>
    /// <param name="searchId">缓存 key。翻页时传入可复用首次搜索结果，无需 keyword</param>
    /// <param name="page">页码，从1开始</param>
    /// <param name="pageSize">每页条数，默认 20，最大 50</param>
    /// <param name="sortBy">排序方式：relevance(默认)/latest/price_asc/price_desc</param>
    [HttpGet("search")]
    public async Task<ActionResult<SearchResultDto>> Search(
        [FromQuery] string? keyword = null,
        [FromQuery] string? searchId = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? sortBy = null)
    {
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
            SortBy = sortBy
        };

        var result = await _searchService.SearchAsync(request);
        return Ok(result);
    }

    /// <summary>
    /// 全量重建TermGraph
    /// </summary>
    [HttpPost("search/rebuild-graph")]
    public async Task<ActionResult> RebuildGraph()
    {
        await _searchService.RebuildGraphAsync();
        return Ok(new { message = "TermGraph rebuild completed" });
    }
}

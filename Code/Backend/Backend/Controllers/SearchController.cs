using Backend.Dtos.Product;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

/// <summary>
/// 商品搜索 API
/// </summary>
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
    /// 商品搜索（支持分词 + 查询扩展 + 相关性排序）
    /// </summary>
    /// <param name="keyword">搜索关键词</param>
    /// <param name="page">页码（从 1 开始），默认 1</param>
    /// <param name="pageSize">每页条数，默认 20，最大 50</param>
    /// <param name="sortBy">排序方式：relevance(默认)/latest/price_asc/price_desc</param>
    [HttpGet("search")]
    public async Task<ActionResult<SearchResultDto>> Search(
        [FromQuery] string keyword,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? sortBy = null)
    {
        if (string.IsNullOrWhiteSpace(keyword))
            return BadRequest(new { message = "搜索关键词不能为空" });

        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 20;
        if (pageSize > 50) pageSize = 50;

        var request = new SearchRequestDto
        {
            Keyword = keyword.Trim(),
            Page = page,
            PageSize = pageSize,
            SortBy = sortBy
        };

        var result = await _searchService.SearchAsync(request);
        return Ok(result);
    }

    /// <summary>
    /// （管理用）触发全量重建 TermGraph
    /// </summary>
    [HttpPost("search/rebuild-graph")]
    public async Task<ActionResult> RebuildGraph()
    {
        await _searchService.RebuildGraphAsync();
        return Ok(new { message = "TermGraph rebuild completed" });
    }
}

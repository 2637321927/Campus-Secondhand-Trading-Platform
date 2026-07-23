namespace Backend.Dtos.Product;

/// <summary>
/// 搜索请求参数
/// </summary>
public class SearchRequestDto
{
    public string Keyword { get; set; } = string.Empty;
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SortBy { get; set; } // "relevance" | "latest" | "price_asc" | "price_desc"
}

/// <summary>
/// 搜索结果中的商品卡片（相比 ProductCardDto 多了相关性分数）
/// </summary>
public class SearchProductCardDto
{
    public long ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string? CoverImageUrl { get; set; }
    public string SellerName { get; set; } = string.Empty;
    public DateTime ReleaseDate { get; set; }
    public int ViewCount { get; set; }
    /// <summary>相关性分数（越高越相关）</summary>
    public int RelevanceScore { get; set; }
}

/// <summary>
/// 搜索响应
/// </summary>
public class SearchResultDto
{
    public List<SearchProductCardDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    /// <summary>本次查询实际使用的扩展词条（调试用）</summary>
    public List<string> ExpandedTerms { get; set; } = new();
}

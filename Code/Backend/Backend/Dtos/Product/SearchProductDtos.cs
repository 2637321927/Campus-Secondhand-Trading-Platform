namespace Backend.Dtos.Product;

public class SearchRequestDto
{
    /// <summary>缓存key</summary>
    public string? SearchId { get; set; }
    public string Keyword { get; set; } = string.Empty;
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    /// <summary>"relevance" | "latest" | "price_asc" | "price_desc"</summary>
    public string? SortBy { get; set; }
}

public class SearchResultDto
{
    /// <summary>本次搜索的缓存key</summary>
    public string SearchId { get; set; } = string.Empty;
    public List<ProductCardDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public List<string> ExpandedTerms { get; set; } = new();
}

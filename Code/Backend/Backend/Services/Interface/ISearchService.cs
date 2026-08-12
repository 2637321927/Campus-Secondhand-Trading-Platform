using Backend.Dtos.Product;

namespace Backend.Services;

public interface ISearchService
{

    Task<SearchResultDto> SearchProductAsync(SearchRequestDto request);

    Task NotifyProductCreatedAsync(long productId);

    Task RebuildGraphAsync();
    
}

using Backend.Dtos.Product;

namespace Backend.Services;

public interface ISearchService
{

    Task<SearchResultDto> SearchAsync(SearchRequestDto request);

    Task NotifyProductCreatedAsync(long productId);

    Task RebuildGraphAsync();
    
}

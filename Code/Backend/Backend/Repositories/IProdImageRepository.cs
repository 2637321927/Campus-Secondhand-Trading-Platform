using Backend.Models;

namespace Backend.Repositories;

public interface IProdImageRepository
{
    Task<ProdImage?> GetByIdAsync(long imgFileId);
    Task<List<ProdImage>> GetByIdsAsync(IEnumerable<long> imgFileIds);
    Task<List<ProdImage>> GetByProductIdAsync(long productId);
    Task AddAsync(ProdImage image);
    void Update(ProdImage image);
    void Delete(ProdImage image);
    Task SaveAsync();
}

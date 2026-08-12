using Backend.Models;

namespace Backend.Repositories;

public interface IRevImageRepository
{
    Task<RevImage?> GetByIdAsync(long imgFileId);
    Task<List<RevImage>> GetByReviewIdAsync(int reviewId);
    Task AddAsync(RevImage image);
    void Update(RevImage image);
    void Delete(RevImage image);
    Task SaveAsync();
}

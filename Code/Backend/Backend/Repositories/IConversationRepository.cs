using Backend.Models;

namespace Backend.Repositories;

public interface IConversationRepository
{
    Task<Conversation?> GetByIdAsync(int sessionId);
    Task<List<Conversation>> GetAllAsync();
    Task<List<Conversation>> GetByBuyerIdAsync(int buyerId);
    Task<List<Conversation>> GetByProductIdAsync(long productId);
    Task AddAsync(Conversation conversation);
    void Update(Conversation conversation);
    void Delete(Conversation conversation);
    Task SaveAsync();
}

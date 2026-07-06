using Backend.Models;

namespace Backend.Repositories;

public interface IMessageRepository
{
    Task<Message?> GetByIdAsync(int sessionId, int msgIndex);
    Task<List<Message>> GetBySessionIdAsync(int sessionId);
    Task AddAsync(Message message);
    void Update(Message message);
    void Delete(Message message);
    Task SaveAsync();
}

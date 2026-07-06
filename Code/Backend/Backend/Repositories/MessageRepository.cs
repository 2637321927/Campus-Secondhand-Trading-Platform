using Backend.Data;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories;

public class MessageRepository : IMessageRepository
{
    private readonly AppDbContext _context;
    public MessageRepository(AppDbContext context) => _context = context;

    public async Task<Message?> GetByIdAsync(int sessionId, int msgIndex)
        => await _context.Messages.FindAsync(sessionId, msgIndex);

    public async Task<List<Message>> GetBySessionIdAsync(int sessionId)
        => await _context.Messages.Where(m => m.SessionId == sessionId)
            .OrderBy(m => m.MsgIndex).ToListAsync();

    public async Task AddAsync(Message message)
        => await _context.Messages.AddAsync(message);

    public void Update(Message message)
        => _context.Messages.Update(message);

    public void Delete(Message message)
        => _context.Messages.Remove(message);

    public async Task SaveAsync()
        => await _context.SaveChangesAsync();
}

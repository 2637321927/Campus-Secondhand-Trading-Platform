using Backend.Data;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories;

public class ConversationRepository : IConversationRepository
{
    private readonly AppDbContext _context;
    public ConversationRepository(AppDbContext context) => _context = context;

    public async Task<Conversation?> GetByIdAsync(int sessionId)
        => await _context.Conversations.FindAsync(sessionId);

    public async Task<List<Conversation>> GetAllAsync()
        => await _context.Conversations.ToListAsync();

    public async Task<List<Conversation>> GetByBuyerIdAsync(int buyerId)
        => await _context.Conversations.Where(c => c.BuyerId == buyerId).ToListAsync();

    public async Task<List<Conversation>> GetByProductIdAsync(long productId)
        => await _context.Conversations.Where(c => c.ProductId == productId).ToListAsync();

    public async Task AddAsync(Conversation conversation)
        => await _context.Conversations.AddAsync(conversation);

    public void Update(Conversation conversation)
        => _context.Conversations.Update(conversation);

    public void Delete(Conversation conversation)
        => _context.Conversations.Remove(conversation);

    public async Task SaveAsync()
        => await _context.SaveChangesAsync();
}

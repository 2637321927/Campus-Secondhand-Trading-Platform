using Backend.Data;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories;

public class AddressRepository : IAddressRepository
{
    private readonly AppDbContext _context;
    public AddressRepository(AppDbContext context) => _context = context;

    public async Task<Address?> GetByIdAsync(int addressId)
        => await _context.Addresses.FindAsync(addressId);

    public async Task<List<Address>> GetByUserIdAsync(int userId)
        => await _context.Addresses.Where(a => a.UserId == userId).ToListAsync();

    public async Task<List<Address>> GetAllAsync()
        => await _context.Addresses.ToListAsync();

    public async Task AddAsync(Address address)
        => await _context.Addresses.AddAsync(address);

    public void Update(Address address)
        => _context.Addresses.Update(address);

    public void Delete(Address address)
        => _context.Addresses.Remove(address);

    public async Task SaveAsync()
        => await _context.SaveChangesAsync();
}

using Backend.Models;

namespace Backend.Repositories;

public interface IAddressRepository
{
    Task<Address?> GetByIdAsync(int addressId);
    Task<List<Address>> GetByUserIdAsync(int userId);
    Task<List<Address>> GetAllAsync();
    Task AddAsync(Address address);
    void Update(Address address);
    void Delete(Address address);
    Task SaveAsync();
}

using Backend.Dtos.User;

namespace Backend.Services;

public interface IAddressService
{
    Task<List<AddressDto>> GetMyAddressesAsync(int userId);
    Task<AddressDto?> GetAddressByIdAsync(int userId, int addressId);
    Task<AddressDto> CreateAddressAsync(int userId, CreateAddressDto dto);
}

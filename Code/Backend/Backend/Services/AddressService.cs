using Backend.Dtos.User;
using Backend.Models;
using Backend.Repositories;

namespace Backend.Services;

public class AddressService : IAddressService
{
    private readonly IAddressRepository _addressRepo;

    public AddressService(IAddressRepository addressRepo)
    {
        _addressRepo = addressRepo;
    }

    public async Task<List<AddressDto>> GetMyAddressesAsync(int userId)
    {
        var addresses = await _addressRepo.GetByUserIdAsync(userId);
        return addresses.Select(ToDto).ToList();
    }

    public async Task<AddressDto?> GetAddressByIdAsync(int userId, int addressId)
    {
        var address = await _addressRepo.GetByIdAsync(addressId);
        if (address == null || address.UserId != userId) return null;
        return ToDto(address);
    }

    public async Task<AddressDto> CreateAddressAsync(int userId, CreateAddressDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new InvalidOperationException("收件人姓名不能为空");
        if (dto.Name.Length > 10)
            throw new InvalidOperationException("收件人姓名不能超过10个字符");
        if (string.IsNullOrWhiteSpace(dto.PhoneNumber))
            throw new InvalidOperationException("手机号不能为空");
        if (dto.PhoneNumber.Length != 11 || !dto.PhoneNumber.All(char.IsDigit))
            throw new InvalidOperationException("手机号必须为11位数字");
        if (string.IsNullOrWhiteSpace(dto.DetailAddress))
            throw new InvalidOperationException("详细地址不能为空");
        if (dto.DetailAddress.Length > 50)
            throw new InvalidOperationException("详细地址不能超过50个字符");

        var existingAddresses = await _addressRepo.GetByUserIdAsync(userId);

        if (dto.IsDefault && existingAddresses.Count > 0)
        {
            await _addressRepo.ResetDefaultByUserIdAsync(userId);
        }

        var isDefault = dto.IsDefault || existingAddresses.Count == 0;

        var address = new Address
        {
            Name = dto.Name.Trim(),
            PhoneNumber = dto.PhoneNumber.Trim(),
            DetailAddress = dto.DetailAddress.Trim(),
            UserId = userId,
            IsDefault = isDefault ? 1 : 0
        };

        await _addressRepo.AddAsync(address);
        await _addressRepo.SaveAsync();

        return ToDto(address);
    }

    private static AddressDto ToDto(Address a) => new()
    {
        AddressId = a.AddressId,
        Name = a.Name,
        PhoneNumber = a.PhoneNumber,
        DetailAddress = a.DetailAddress,
        IsDefault = a.IsDefault == 1
    };
}

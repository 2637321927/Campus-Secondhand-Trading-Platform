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
        var list = await _addressRepo.GetByUserIdAsync(userId);
        // 默认地址排前，其余按主键倒序（新的在前）
        return list
            .OrderByDescending(a => a.IsDefault)
            .ThenByDescending(a => a.AddressId)
            .Select(ToDto)
            .ToList();
    }

    public async Task<AddressDto> CreateAsync(int userId, CreateAddressDto dto)
    {
        // 基本校验
        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new InvalidOperationException("收货人姓名不能为空");
        if (string.IsNullOrWhiteSpace(dto.PhoneNumber))
            throw new InvalidOperationException("手机号不能为空");
        if (string.IsNullOrWhiteSpace(dto.DetailAddress))
            throw new InvalidOperationException("详细地址不能为空");

        var all = await _addressRepo.GetByUserIdAsync(userId);

        // 第一条地址自动设为默认
        bool shouldBeDefault = dto.IsDefault || all.Count == 0;

        // 如果新地址要设为默认，需要把其他地址的默认标记清除
        if (shouldBeDefault)
        {
            foreach (var a in all.Where(a => a.IsDefault == 1))
            {
                a.IsDefault = 0;
                _addressRepo.Update(a);
            }
        }

        var address = new Address
        {
            UserId = userId,
            Name = dto.Name.Trim(),
            PhoneNumber = dto.PhoneNumber.Trim(),
            DetailAddress = dto.DetailAddress.Trim(),
            IsDefault = shouldBeDefault ? 1 : 0
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

    public async Task<AddressDto?> GetByIdAsync(int userId, int addressId)
    {
        var address = await _addressRepo.GetByIdAsync(addressId);
        if (address == null || address.UserId != userId)
            return null;

        return ToDto(address);
    }

    public async Task<AddressDto> UpdateAsync(int userId, int addressId, UpdateAddressDto dto)
    {
        var address = await _addressRepo.GetByIdAsync(addressId);
        if (address == null || address.UserId != userId)
            throw new InvalidOperationException("地址不存在");

        // 可选字段更新（不提供则保持原值）
        if (dto.Name != null)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new InvalidOperationException("收货人姓名不能为空");
            address.Name = dto.Name.Trim();
        }
        if (dto.PhoneNumber != null)
        {
            if (string.IsNullOrWhiteSpace(dto.PhoneNumber))
                throw new InvalidOperationException("手机号不能为空");
            address.PhoneNumber = dto.PhoneNumber.Trim();
        }
        if (dto.DetailAddress != null)
        {
            if (string.IsNullOrWhiteSpace(dto.DetailAddress))
                throw new InvalidOperationException("详细地址不能为空");
            address.DetailAddress = dto.DetailAddress.Trim();
        }

        _addressRepo.Update(address);
        await _addressRepo.SaveAsync();

        return ToDto(address);
    }

    public async Task DeleteAsync(int userId, int addressId)
    {
        var address = await _addressRepo.GetByIdAsync(addressId);
        if (address == null || address.UserId != userId)
            throw new InvalidOperationException("地址不存在");

        var wasDefault = address.IsDefault == 1;

        _addressRepo.Delete(address);

        // 如果删的是默认地址，把剩余最新的一条顶上
        if (wasDefault)
        {
            var remaining = await _addressRepo.GetByUserIdAsync(userId);
            var next = remaining.OrderByDescending(a => a.AddressId).FirstOrDefault();
            if (next != null)
            {
                next.IsDefault = 1;
                _addressRepo.Update(next);
            }
        }

        await _addressRepo.SaveAsync();
    }

    public async Task<AddressDto> SetDefaultAsync(int userId, int addressId)
    {
        var address = await _addressRepo.GetByIdAsync(addressId);
        if (address == null || address.UserId != userId)
            throw new InvalidOperationException("地址不存在");

        // 清除同用户其他地址的默认标记
        var all = await _addressRepo.GetByUserIdAsync(userId);
        foreach (var a in all.Where(a => a.AddressId != addressId && a.IsDefault == 1))
        {
            a.IsDefault = 0;
            _addressRepo.Update(a);
        }

        address.IsDefault = 1;
        _addressRepo.Update(address);
        await _addressRepo.SaveAsync();

        return ToDto(address);
    }

    public async Task<AddressDto?> GetDefaultAsync(int userId)
    {
        var list = await _addressRepo.GetByUserIdAsync(userId);
        var def = list.FirstOrDefault(a => a.IsDefault == 1);
        return def == null ? null : ToDto(def);
    }
}

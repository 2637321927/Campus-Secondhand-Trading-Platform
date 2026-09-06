using Backend.Dtos.User;

namespace Backend.Services;

public interface IAddressService
{
    /// <summary>
    /// 获取当前用户的地址列表，默认地址排最前
    /// </summary>
    Task<List<AddressDto>> GetMyAddressesAsync(int userId);

    /// <summary>
    /// 新增收货/交易地址
    /// </summary>
    Task<AddressDto> CreateAsync(int userId, CreateAddressDto dto);

    /// <summary>
    /// 获取单个地址详情（必须是当前用户的地址）
    /// </summary>
    Task<AddressDto?> GetByIdAsync(int userId, int addressId);

    /// <summary>
    /// 修改地址（必须是当前用户的地址）
    /// </summary>
    Task<AddressDto> UpdateAsync(int userId, int addressId, UpdateAddressDto dto);

    /// <summary>
    /// 删除地址（必须是当前用户的地址）
    /// </summary>
    Task DeleteAsync(int userId, int addressId);

    /// <summary>
    /// 设置指定地址为默认地址（必须是当前用户的地址）
    /// </summary>
    Task<AddressDto> SetDefaultAsync(int userId, int addressId);

    /// <summary>
    /// 获取当前用户的默认地址
    /// </summary>
    Task<AddressDto?> GetDefaultAsync(int userId);
}

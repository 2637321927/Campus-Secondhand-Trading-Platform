using Backend.Dtos.User;

namespace Backend.Services;

public interface IBaseUserService
{
    Task<UserDto> RegisterAsync(RegisterDto dto);
    Task<UserDto?> GetByIdAsync(int userId);
}

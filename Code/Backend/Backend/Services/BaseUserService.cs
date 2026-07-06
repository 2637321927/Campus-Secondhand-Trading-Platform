using Backend.Dtos.User;
using Backend.Models;
using Backend.Repositories;

namespace Backend.Services;

public class BaseUserService : IBaseUserService
{
    private readonly IBaseUserRepository _userRepo;
    private readonly INormUserRepository _normUserRepo;

    public BaseUserService(IBaseUserRepository userRepo, INormUserRepository normUserRepo)
    {
        _userRepo = userRepo;
        _normUserRepo = normUserRepo;
    }

    public async Task<UserDto> RegisterAsync(RegisterDto dto)
    {
        // 1. 创建基础用户
        var baseUser = new BaseUser
        {
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password), // 密码加密
            PhoneNumber = dto.PhoneNumber,
            UserType = 0,  // 普通用户
            Gender = "unknown",
            RegisterTime = DateTime.Now
        };
        await _userRepo.AddAsync(baseUser);
        await _userRepo.SaveAsync();

        // 2. 创建普通用户扩展信息
        var normUser = new NormUser
        {
            UserId = baseUser.UserId,
            UserName = dto.UserName,
            Credit = 100
        };
        await _normUserRepo.AddAsync(normUser);
        await _normUserRepo.SaveAsync();

        return new UserDto
        {
            UserId = baseUser.UserId,
            Email = baseUser.Email,
            PhoneNumber = baseUser.PhoneNumber,
            UserName = normUser.UserName,
            RegisterTime = baseUser.RegisterTime
        };
    }

    public async Task<UserDto?> GetByIdAsync(int userId)
    {
        var normUser = await _normUserRepo.GetByIdAsync(userId);
        if (normUser?.BaseUser == null) return null;

        return new UserDto
        {
            UserId = normUser.UserId,
            Email = normUser.BaseUser.Email,
            PhoneNumber = normUser.BaseUser.PhoneNumber,
            UserName = normUser.UserName,
            RegisterTime = normUser.BaseUser.RegisterTime
        };
    }
}

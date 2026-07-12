using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Backend.Dtos.Auth;
using Backend.Dtos.User;
using Backend.Models;
using Backend.Repositories;
using Microsoft.IdentityModel.Tokens;

namespace Backend.Services;

public class BaseUserService : IBaseUserService
{
    private readonly IBaseUserRepository _userRepo;
    private readonly INormUserRepository _normUserRepo;
    private readonly IAdminUserRepository _adminUserRepo;
    private readonly IConfiguration _configuration;

    //简单内存存储：重置密码用临时token（生产环境应存数据库或用Redis）
    private static readonly Dictionary<string, (string ResetToken, DateTime ExpireTime)> _resetTokens = new();

    public BaseUserService(
        IBaseUserRepository userRepo,
        INormUserRepository normUserRepo,
        IAdminUserRepository adminUserRepo,
        IConfiguration configuration)
    {
        _userRepo = userRepo;
        _normUserRepo = normUserRepo;
        _adminUserRepo = adminUserRepo;
        _configuration = configuration;
    }

    //注册
    public async Task<UserDto> RegisterAsync(RegisterDto dto)
    {
        //检查邮箱是否已存在
        var existingEmail = await _userRepo.GetByEmailAsync(dto.Email);
        if (existingEmail != null)
            throw new InvalidOperationException("该邮箱已被注册");

        //检查手机号是否已存在
        if (!string.IsNullOrEmpty(dto.PhoneNumber))
        {
            var existingPhone = await _userRepo.GetByPhoneAsync(dto.PhoneNumber);
            if (existingPhone != null)
                throw new InvalidOperationException("该手机号已被注册");
        }

        //创建基础用户
        var baseUser = new BaseUser
        {
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            PhoneNumber = dto.PhoneNumber,
            UserType = 0,
            Gender = "unknown",
            RegisterTime = DateTime.Now
        };
        await _userRepo.AddAsync(baseUser);
        await _userRepo.SaveAsync();

        //创建普通用户扩展信息
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

    //登录
    public async Task<LoginResponseDto> LoginAsync(LoginDto dto)
    {
        //优先用邮箱登录，其次手机号
        BaseUser? user;
        if (!string.IsNullOrEmpty(dto.Email))
            user = await _userRepo.GetByEmailAsync(dto.Email);
        else if (!string.IsNullOrEmpty(dto.PhoneNumber))
            user = await _userRepo.GetByPhoneAsync(dto.PhoneNumber);
        else
            throw new InvalidOperationException("请输入邮箱或手机号");

        if (user == null)
            throw new InvalidOperationException("账号或密码错误");

        if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            throw new InvalidOperationException("邮箱或密码错误");

        if (user.IsBanned == 1 && (user.BannedUntil == null || user.BannedUntil > DateTime.Now))
            throw new InvalidOperationException("该账号已被封禁");

        //获取用户名
        string userName = "";
        if (user.UserType == 0)
        {
            var normUser = await _normUserRepo.GetByIdAsync(user.UserId);
            userName = normUser?.UserName ?? "";
        }
        else
        {
            userName = "管理员";
        }

        //生成JWT Token
        var token = GenerateJwtToken(user.UserId, user.UserType, user.Email);

        return new LoginResponseDto
        {
            Token = token,
            UserId = user.UserId,
            UserType = user.UserType,
            UserName = userName
        };
    }

    //获取当前用户信息
    public async Task<MeResponseDto?> GetMeAsync(int userId)
    {
        var user = await _userRepo.GetByIdAsync(userId);
        if (user == null) return null;

        string userName = "";
        if (user.UserType == 0)
        {
            var normUser = await _normUserRepo.GetByIdAsync(userId);
            userName = normUser?.UserName ?? "";
        }
        else
        {
            userName = "管理员";
        }

        return new MeResponseDto
        {
            UserId = user.UserId,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            UserName = userName,
            UserType = user.UserType,
            Gender = user.Gender,
            AvatarFileId = user.AvatarFileId,
            IsBanned = user.IsBanned,
            BannedUntil = user.BannedUntil
        };
    }

    //修改密码
    public async Task ChangePasswordAsync(int userId, ChangePasswordDto dto)
    {
        var user = await _userRepo.GetByIdAsync(userId);
        if (user == null)
            throw new InvalidOperationException("用户不存在");

        if (!BCrypt.Net.BCrypt.Verify(dto.OldPassword, user.PasswordHash))
            throw new InvalidOperationException("原密码错误");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        _userRepo.Update(user);
        await _userRepo.SaveAsync();
    }

    //发起重置密码
    public async Task RequestPasswordResetAsync(ResetPasswordRequestDto dto)
    {
        //优先邮箱，其次手机号
        BaseUser? user;
        string identifier;
        if (!string.IsNullOrEmpty(dto.Email))
        {
            user = await _userRepo.GetByEmailAsync(dto.Email);
            identifier = dto.Email;
        }
        else if (!string.IsNullOrEmpty(dto.PhoneNumber))
        {
            user = await _userRepo.GetByPhoneAsync(dto.PhoneNumber);
            identifier = dto.PhoneNumber;
        }
        else
        {
            return;
        }

        if (user == null)
            //无论用户是否存在都返回成功，防止枚举攻击
            return;

        //生成6位数字验证码
        var resetToken = new Random().Next(100000, 999999).ToString();
        _resetTokens[identifier] = (resetToken, DateTime.Now.AddMinutes(15));

        //TODO: 实际项目应发送邮件/短信，这里先打印到控制台
        Console.WriteLine($"[密码重置] 账号: {identifier}, 验证码: {resetToken}");
    }

    //确认重置密码
    public async Task ConfirmPasswordResetAsync(ResetPasswordConfirmDto dto)
    {
        //确定用哪个标识符查找验证码
        var identifier = !string.IsNullOrEmpty(dto.Email) ? dto.Email : dto.PhoneNumber;
        if (string.IsNullOrEmpty(identifier))
            throw new InvalidOperationException("请提供邮箱或手机号");

        if (!_resetTokens.TryGetValue(identifier, out var stored))
            throw new InvalidOperationException("未发起重置请求或验证码已过期");

        if (stored.ExpireTime < DateTime.Now)
        {
            _resetTokens.Remove(identifier);
            throw new InvalidOperationException("验证码已过期，请重新发起");
        }

        if (stored.ResetToken != dto.ResetToken)
            throw new InvalidOperationException("验证码错误");

        //用同样的方式查找用户
        BaseUser? user;
        if (!string.IsNullOrEmpty(dto.Email))
            user = await _userRepo.GetByEmailAsync(dto.Email);
        else
            user = await _userRepo.GetByPhoneAsync(dto.PhoneNumber!);

        if (user == null)
            throw new InvalidOperationException("用户不存在");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        _userRepo.Update(user);
        await _userRepo.SaveAsync();

        _resetTokens.Remove(identifier);
    }

    //权限检查
    public async Task<bool> CheckPermissionAsync(int userId, string requiredPermission)
    {
        var user = await _userRepo.GetByIdAsync(userId);
        if (user == null) return false;

        return requiredPermission.ToLower() switch
        {
            "admin" => user.UserType == 1,
            "user" => true,
            "seller" => user.UserType == 0,
            _ => false
        };
    }

    //根据ID获取
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

    //生成JWT Token（私有方法）
    private string GenerateJwtToken(int userId, int userType, string email)
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim("userId", userId.ToString()),
            new Claim("userType", userType.ToString()),
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.Role, userType == 1 ? "Admin" : "User")
        };

        var expireHours = int.Parse(_configuration["Jwt:ExpireHours"] ?? "72");
        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddHours(expireHours),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

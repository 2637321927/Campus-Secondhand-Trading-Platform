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
    private readonly IUpdatedFileService _updatedFileService;

    //简单内存存储：重置密码用临时token（生产环境应存数据库或用Redis）
    private static readonly Dictionary<string, (string ResetToken, DateTime ExpireTime)> _resetTokens = new();

    // 允许的头像 MIME 类型
    private static readonly HashSet<string> AllowedAvatarMimeTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg", "image/png", "image/gif", "image/bmp", "image/webp"
    };

    // 头像大小上限：10MB
    private const long MaxAvatarSize = 10 * 1024 * 1024;

    public BaseUserService(
        IBaseUserRepository userRepo,
        INormUserRepository normUserRepo,
        IAdminUserRepository adminUserRepo,
        IConfiguration configuration,
        IUpdatedFileService updatedFileService)
    {
        _userRepo = userRepo;
        _normUserRepo = normUserRepo;
        _adminUserRepo = adminUserRepo;
        _configuration = configuration;
        _updatedFileService = updatedFileService;
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

    //获取个人中心资料
    public async Task<UserProfileDto?> GetProfileAsync(int userId)
    {
        var normUser = await _normUserRepo.GetByIdAsync(userId);
        if (normUser?.BaseUser == null) return null;

        return new UserProfileDto
        {
            UserId = normUser.UserId,
            Email = normUser.BaseUser.Email,
            PhoneNumber = normUser.BaseUser.PhoneNumber,
            UserName = normUser.UserName,
            Gender = normUser.BaseUser.Gender,
            Profile = normUser.Profile,
            AvatarFileId = normUser.BaseUser.AvatarFileId,
            Credit = normUser.Credit,
            RegisterTime = normUser.BaseUser.RegisterTime
        };
    }

    //修改个人中心资料（支持部分更新）
    public async Task<UserProfileDto?> UpdateProfileAsync(int userId, UpdateProfileDto dto)
    {
        var normUser = await _normUserRepo.GetByIdAsync(userId);
        if (normUser?.BaseUser == null) return null;

        var baseUser = normUser.BaseUser;

        // 用户名：长度不超过 20
        if (dto.UserName != null)
        {
            if (dto.UserName.Length > 20)
                throw new InvalidOperationException("用户名长度不能超过 20 个字符");
            normUser.UserName = dto.UserName;
        }

        // 手机号：必须为 11 位数字且唯一
        if (dto.PhoneNumber != null)
        {
            if (dto.PhoneNumber.Length != 11 || !dto.PhoneNumber.All(char.IsDigit))
                throw new InvalidOperationException("手机号必须为 11 位数字");

            if (!string.Equals(dto.PhoneNumber, baseUser.PhoneNumber, StringComparison.Ordinal))
            {
                var existing = await _userRepo.GetByPhoneAsync(dto.PhoneNumber);
                if (existing != null && existing.UserId != userId)
                    throw new InvalidOperationException("该手机号已被其他用户使用");
                baseUser.PhoneNumber = dto.PhoneNumber;
            }
        }

        // 性别：仅允许 male/female/unknown
        if (dto.Gender != null)
        {
            var g = dto.Gender.ToLowerInvariant();
            if (g != "male" && g != "female" && g != "unknown")
                throw new InvalidOperationException("性别只能为 male、female 或 unknown");
            baseUser.Gender = g;
        }

        // 个性签名：长度不超过 20
        if (dto.Profile != null)
        {
            if (dto.Profile.Length > 20)
                throw new InvalidOperationException("个性签名长度不能超过 20 个字符");
            normUser.Profile = dto.Profile;
        }

        _userRepo.Update(baseUser);
        _normUserRepo.Update(normUser);
        await _userRepo.SaveAsync();

        return await GetProfileAsync(userId);
    }

    //上传/更换当前用户头像
    public async Task<AvatarUploadResponseDto> UploadAvatarAsync(int userId, IFormFile file)
    {
        if (file == null || file.Length == 0)
            throw new InvalidOperationException("请选择要上传的头像文件");

        if (!AllowedAvatarMimeTypes.Contains(file.ContentType))
            throw new InvalidOperationException("仅支持 jpeg/png/gif/bmp/webp 格式的图片");

        if (file.Length > MaxAvatarSize)
            throw new InvalidOperationException("头像文件大小不能超过 10MB");

        var user = await _userRepo.GetByIdAsync(userId)
            ?? throw new InvalidOperationException("用户不存在");

        // 若用户已有旧头像且不是默认头像，先删除旧文件
        var defaultAvatarId = _configuration.GetValue<long?>("AppDefaults:DefaultAvatarFileId") ?? 1L;
        var oldAvatarFileId = user.AvatarFileId;
        if (oldAvatarFileId != null && oldAvatarFileId != defaultAvatarId)
        {
            try
            {
                await _updatedFileService.HardDeleteAsync(oldAvatarFileId.Value);
            }
            catch (FileNotFoundException)
            {
                // 物理文件已不存在，尝试软删除 DB 记录
                try { await _updatedFileService.SoftDeleteAsync(oldAvatarFileId.Value); }
                catch { /* 忽略：旧记录可能已被清理 */ }
            }
            catch (KeyNotFoundException)
            {
                // DB 记录已不存在，无需处理
            }
        }

        // 上传新头像文件
        var uploaded = await _updatedFileService.UploadMultipleAsync(
            new List<IFormFile> { file }, userId);

        if (uploaded.Count == 0)
            throw new InvalidOperationException("头像上传失败");

        var newFile = uploaded[0];

        // 更新用户头像引用
        user.AvatarFileId = newFile.FileId;
        _userRepo.Update(user);
        await _userRepo.SaveAsync();

        return new AvatarUploadResponseDto
        {
            AvatarFileId = newFile.FileId,
            FileName = newFile.FileName
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

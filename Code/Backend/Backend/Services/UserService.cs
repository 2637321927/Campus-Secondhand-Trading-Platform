using Backend.Data;
using Backend.Dtos.User;
using Backend.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Text.RegularExpressions;

namespace Backend.Services;

/// <summary>
/// 用户中心服务实现（个人中心资料管理）
/// </summary>
public class UserService : IUserService
{
    private readonly INormUserRepository _normUserRepo;
    private readonly IBaseUserRepository _baseUserRepo;
    private readonly IUpdatedFileService _updatedFileService;
    private readonly AppDbContext _context;
    private readonly long _defaultAvatarFileId;

    private static readonly string[] AllowedGenders = { "male", "female", "unknown" };
    private static readonly HashSet<string> AllowedAvatarMimeTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg", "image/png", "image/gif", "image/bmp", "image/webp"
    };

    public UserService(
        INormUserRepository normUserRepo,
        IBaseUserRepository baseUserRepo,
        IUpdatedFileService updatedFileService,
        IConfiguration configuration,
        AppDbContext context)
    {
        _normUserRepo = normUserRepo;
        _baseUserRepo = baseUserRepo;
        _updatedFileService = updatedFileService;
        _context = context;
        _defaultAvatarFileId = configuration.GetValue<long>("AppDefaults:DefaultAvatarFileId");
    }

    /// <summary>
    /// 获取当前用户的个人中心资料
    /// </summary>
    public async Task<UserProfileDto?> GetProfileAsync(int userId)
    {
        // 1. 获取用户基础信息（含 BaseUser 导航属性）
        var normUser = await _normUserRepo.GetByIdAsync(userId);
        if (normUser?.BaseUser == null) return null;

        var baseUser = normUser.BaseUser;

        // 2. 统计数据（顺序查询，避免 DbContext 并发问题）
        var publishedCount = await _context.Products
            .CountAsync(p => p.UserId == userId);

        var soldCount = await _context.Products
            .CountAsync(p => p.UserId == userId && p.Status == "sold");

        var purchaseCount = await _context.Purchases
            .CountAsync(p => p.BuyerId == userId);

        var favoriteCount = await _context.Collections
            .CountAsync(c => c.UserId == userId);

        // 3. 组装返回 DTO
        return new UserProfileDto
        {
            UserId = baseUser.UserId,
            Email = baseUser.Email,
            PhoneNumber = baseUser.PhoneNumber,
            Gender = baseUser.Gender,
            AvatarFileId = baseUser.AvatarFileId,
            RegisterTime = baseUser.RegisterTime,
            UserName = normUser.UserName,
            Credit = normUser.Credit,
            Profile = normUser.Profile,
            DashboardCounts = new DashboardCountsDto
            {
                PublishedCount = publishedCount,
                SoldCount = soldCount,
                PurchaseCount = purchaseCount,
                FavoriteCount = favoriteCount
            }
        };
    }

    /// <summary>
    /// 修改当前用户的个人资料
    /// </summary>
    public async Task<UserProfileDto> UpdateProfileAsync(int userId, UpdateProfileDto dto)
    {
        // 1. 获取用户信息
        var normUser = await _normUserRepo.GetByIdAsync(userId);
        if (normUser?.BaseUser == null)
            throw new InvalidOperationException("用户不存在");

        var baseUser = normUser.BaseUser;

        // 2. 验证并更新 BaseUser 表字段
        if (dto.PhoneNumber != null)
        {
            // 手机号格式验证：11位数字
            if (!Regex.IsMatch(dto.PhoneNumber, @"^\d{11}$"))
                throw new InvalidOperationException("手机号必须为11位数字");

            // 手机号唯一性检查
            var existing = await _baseUserRepo.GetByPhoneAsync(dto.PhoneNumber);
            if (existing != null && existing.UserId != userId)
                throw new InvalidOperationException("该手机号已被其他用户注册");

            baseUser.PhoneNumber = dto.PhoneNumber;
        }

        if (dto.Gender != null)
        {
            if (!AllowedGenders.Contains(dto.Gender))
                throw new InvalidOperationException("性别只能是 male/female/unknown");

            baseUser.Gender = dto.Gender;
        }

        _baseUserRepo.Update(baseUser);
        await _baseUserRepo.SaveAsync();

        // 3. 验证并更新 NormUser 表字段
        if (dto.UserName != null)
        {
            if (dto.UserName.Length > 20)
                throw new InvalidOperationException("昵称最多20个字符");

            normUser.UserName = dto.UserName;
        }

        if (dto.Profile != null)
        {
            if (dto.Profile.Length > 20)
                throw new InvalidOperationException("个人简介最多20个字符");

            normUser.Profile = dto.Profile;
        }

        _normUserRepo.Update(normUser);
        await _normUserRepo.SaveAsync();

        // 4. 返回更新后的完整资料
        var profile = await GetProfileAsync(userId);
        if (profile == null)
            throw new InvalidOperationException("获取更新后的资料失败");

        return profile;
    }

    /// <summary>
    /// 上传或更换当前用户头像
    /// </summary>
    public async Task<UserProfileDto> UpdateAvatarAsync(int userId, IFormFile file)
    {
        // 1. 验证文件
        if (file == null || file.Length == 0)
            throw new InvalidOperationException("请上传头像文件");

        if (!AllowedAvatarMimeTypes.Contains(file.ContentType))
            throw new InvalidOperationException("仅支持 jpeg/png/gif/bmp/webp 格式的图片");

        // 2. 获取用户信息
        var baseUser = await _baseUserRepo.GetByIdAsync(userId);
        if (baseUser == null)
            throw new InvalidOperationException("用户不存在");

        // 3. 上传新文件
        var uploadedFile = await _updatedFileService.UploadAsync(file, userId);

        // 4. 删除旧头像（非默认头像时）
        if (baseUser.AvatarFileId.HasValue && baseUser.AvatarFileId.Value != _defaultAvatarFileId)
        {
            try
            {
                await _updatedFileService.HardDeleteAsync(baseUser.AvatarFileId.Value);
            }
            catch
            {
                // 旧头像删除失败不影响新头像更新，仅记录
            }
        }

        // 5. 更新用户头像
        baseUser.AvatarFileId = uploadedFile.FileId;
        _baseUserRepo.Update(baseUser);
        await _baseUserRepo.SaveAsync();

        // 6. 返回更新后的资料
        var profile = await GetProfileAsync(userId);
        if (profile == null)
            throw new InvalidOperationException("获取更新后的资料失败");

        return profile;
    }
}

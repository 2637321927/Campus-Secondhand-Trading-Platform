using Backend.Dtos.Communication;
using Backend.Models;
using Backend.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services;

public class AnnouncementService : IAnnouncementService
{
    private readonly IAnnouncementRepository _announcementRepo;
    private readonly IAdminUserRepository _adminUserRepo;

    public AnnouncementService(
        IAnnouncementRepository announcementRepo,
        IAdminUserRepository adminUserRepo)
    {
        _announcementRepo = announcementRepo;
        _adminUserRepo = adminUserRepo;
    }

    public async Task<AnnouncementDto> CreateAsync(int adminId, CreateAnnouncementDto dto)
    {
        await EnsureAdminUserAsync(adminId);

        var status = NormalizeStatus(dto.Status);
        var announcement = new Announcement
        {
            Title = dto.Title.Trim(),
            Content = dto.Content.Trim(),
            IsPinned = dto.IsPinned,
            Status = status,
            ReleaseTime = DateTime.Now,
            PublishTime = status == "published" ? DateTime.Now : null,
            AdminId = adminId
        };

        await _announcementRepo.AddAsync(announcement);
        await _announcementRepo.SaveAsync();

        return ToDto(announcement);
    }

    public async Task<AnnouncementDto?> GetByIdAsync(int announcementId)
    {
        var announcement = await _announcementRepo.GetByIdAsync(announcementId);
        return announcement == null ? null : ToDto(announcement);
    }

    public async Task<AnnouncementPageDto> GetPageAsync(
        string? keyword,
        string? status,
        int page,
        int pageSize)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 20 : pageSize;
        pageSize = pageSize > 100 ? 100 : pageSize;

        var query = _announcementRepo.Query();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var kw = keyword.Trim();
            query = query.Where(a => a.Title.Contains(kw) || a.Content.Contains(kw));
        }

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(a => a.Status == status.Trim());

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(a => a.IsPinned)
            .ThenByDescending(a => a.ReleaseTime)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new AnnouncementPageDto
        {
            Items = items.Select(ToDto).ToList(),
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<AnnouncementDto?> UpdateAsync(
        int announcementId,
        UpdateAnnouncementDto dto)
    {
        var announcement = await _announcementRepo.GetByIdAsync(announcementId);
        if (announcement == null) return null;

        if (!string.IsNullOrWhiteSpace(dto.Title))
            announcement.Title = dto.Title.Trim();

        if (!string.IsNullOrWhiteSpace(dto.Content))
            announcement.Content = dto.Content.Trim();

        if (dto.IsPinned.HasValue)
            announcement.IsPinned = dto.IsPinned.Value;

        if (!string.IsNullOrWhiteSpace(dto.Status))
        {
            var status = NormalizeStatus(dto.Status);
            if (announcement.Status != "published" && status == "published")
            {
                announcement.ReleaseTime = DateTime.Now;
                announcement.PublishTime ??= DateTime.Now;
            }
            announcement.Status = status;
        }

        _announcementRepo.Update(announcement);
        await _announcementRepo.SaveAsync();
        return ToDto(announcement);
    }

    public async Task<AnnouncementDto?> SetStatusAsync(int announcementId, string status)
    {
        var announcement = await _announcementRepo.GetByIdAsync(announcementId);
        if (announcement == null) return null;

        var normalized = NormalizeStatus(status);
        announcement.Status = normalized;

        if (normalized == "published")
        {
            announcement.ReleaseTime = DateTime.Now;
            announcement.PublishTime ??= DateTime.Now;
        }

        _announcementRepo.Update(announcement);
        await _announcementRepo.SaveAsync();
        return ToDto(announcement);
    }

    public async Task<bool> DeleteAsync(int announcementId)
    {
        var announcement = await _announcementRepo.GetByIdAsync(announcementId);
        if (announcement == null) return false;

        _announcementRepo.Delete(announcement);
        await _announcementRepo.SaveAsync();
        return true;
    }

    public async Task<AnnouncementStatisticsDto> GetStatisticsAsync()
    {
        return new AnnouncementStatisticsDto
        {
            Total = await _announcementRepo.Query().CountAsync(),
            Published = await _announcementRepo.Query().CountAsync(a => a.Status == "published"),
            Draft = await _announcementRepo.Query().CountAsync(a => a.Status == "draft"),
            Archived = await _announcementRepo.Query().CountAsync(a => a.Status == "archived")
        };
    }

    private static AnnouncementDto ToDto(Announcement a) => new()
    {
        AnnouncementId = a.AnnouncementId,
        Title = a.Title,
        Content = a.Content,
        IsPinned = a.IsPinned,
        Status = a.Status,
        ReleaseTime = a.ReleaseTime,
        PublishTime = a.PublishTime,
        AdminId = a.AdminId
    };

    private static string NormalizeStatus(string status)
        => status switch
        {
            "draft" => "draft",
            "archived" => "archived",
            _ => "published"
        };

    private async Task EnsureAdminUserAsync(int adminId)
    {
        if (await _adminUserRepo.GetByIdAsync(adminId) == null)
        {
            await _adminUserRepo.AddAsync(new AdminUser { UserId = adminId, Permission = 1 });
            await _adminUserRepo.SaveAsync();
        }
    }
}

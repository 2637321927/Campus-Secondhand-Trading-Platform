using Backend.Dtos.Communication;
using Backend.Models;
using Backend.Repositories;

namespace Backend.Services;

public class AnnouncementService : IAnnouncementService
{
    private readonly IAnnouncementRepository _announcementRepo;

    public AnnouncementService(IAnnouncementRepository announcementRepo)
    {
        _announcementRepo = announcementRepo;
    }

    public async Task<AnnouncementDto> CreateAsync(int adminId, CreateAnnouncementDto dto)
    {
        var announcement = new Announcement
        {
            Title = dto.Title.Trim(),
            Info = dto.Info.Trim(),
            ReleaseTime = DateTime.Now,
            AdminId = adminId
        };

        await _announcementRepo.AddAsync(announcement);
        await _announcementRepo.SaveAsync();

        return ToDto(announcement);
    }

    public async Task<List<AnnouncementDto>> GetAllAsync()
    {
        var announcements = await _announcementRepo.GetAllAsync();
        return announcements
            .OrderByDescending(a => a.ReleaseTime)
            .Select(ToDto)
            .ToList();
    }

    private static AnnouncementDto ToDto(Announcement a) => new()
    {
        AnnouncementId = a.AnnouncementId,
        Title = a.Title,
        Info = a.Info,
        ReleaseTime = a.ReleaseTime,
        AdminId = a.AdminId
    };
}

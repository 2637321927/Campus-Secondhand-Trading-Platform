using Backend.Dtos.Communication;

namespace Backend.Services;

public interface IAnnouncementService
{
    Task<AnnouncementDto> CreateAsync(int adminId, CreateAnnouncementDto dto);
    Task<AnnouncementDto?> GetByIdAsync(int announcementId);
    Task<AnnouncementPageDto> GetPageAsync(string? keyword, string? status, int page, int pageSize);
    Task<AnnouncementDto?> UpdateAsync(int announcementId, UpdateAnnouncementDto dto);
    Task<AnnouncementDto?> SetStatusAsync(int announcementId, string status);
    Task<bool> DeleteAsync(int announcementId);
    Task<AnnouncementStatisticsDto> GetStatisticsAsync();
}

using Backend.Dtos.Communication;

namespace Backend.Services;

public interface IAnnouncementService
{
    Task<AnnouncementDto> CreateAsync(int adminId, CreateAnnouncementDto dto);
    Task<List<AnnouncementDto>> GetAllAsync();
}

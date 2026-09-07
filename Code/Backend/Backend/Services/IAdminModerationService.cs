using Backend.Dtos.Admin;

namespace Backend.Services;

public interface IAdminModerationService
{
    Task<AdminModerationPageDto> GetReportsAsync(
        string? keyword,
        string? status,
        string? targetType,
        int page,
        int pageSize);

    Task<AdminModerationPageDto> GetAppealsAsync(
        string? keyword,
        string? status,
        string? targetType,
        int page,
        int pageSize);

    Task<AdminModerationDetailDto?> GetReportDetailAsync(long reportId);
    Task<AdminModerationDetailDto?> GetAppealDetailAsync(long appealId);
    Task<AdminModerationDetailDto?> AcceptReportAsync(long reportId, int adminId);
    Task<AdminModerationDetailDto?> RejectReportAsync(long reportId, int adminId);
    Task<AdminModerationDetailDto?> HandleReportAsync(long reportId, HandleWorkOrderDto dto, int adminId);
    Task<AdminModerationDetailDto?> ApproveAppealAsync(long appealId, int adminId);
    Task<AdminModerationDetailDto?> RejectAppealAsync(long appealId, int adminId);
    Task<AdminModerationDetailDto?> ReplyAppealAsync(long appealId, WorkOrderReplyDto dto, int adminId);
    Task<AdminModerationTasksDto> GetTasksAsync();
}

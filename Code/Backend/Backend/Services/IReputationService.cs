using Backend.Dtos.Reputation;

namespace Backend.Services;

public interface IReputationService
{
    /// <summary>
    /// 获取用户信誉概览
    /// </summary>
    Task<ReputationSummaryDto> GetReputationSummaryAsync(int userId);

    /// <summary>
    /// 获取用户信誉明细
    /// </summary>
    Task<ReputationDetailDto> GetReputationDetailAsync(int userId);
}

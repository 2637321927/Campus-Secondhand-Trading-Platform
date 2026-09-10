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

    /// <summary>
    /// 调整用户信誉值（delta 可正可负，内部钳制到 [CreditRules.Min, CreditRules.Max]）。
    /// 无 NormUser 记录的用户（如管理员）静默跳过。
    /// </summary>
    Task ChangeCreditAsync(int userId, int delta);
}

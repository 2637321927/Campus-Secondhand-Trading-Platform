using Backend.Dtos.Reputation;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

/// <summary>
/// 信誉模块
/// </summary>
[ApiController]
[Route("api")]
public class ReputationController : ControllerBase
{
    private readonly IReputationService _reputationService;

    public ReputationController(IReputationService reputationService)
    {
        _reputationService = reputationService;
    }

    /// <summary>
    /// 获取用户信誉概览
    /// </summary>
    [HttpGet("users/{userId}/reputation/summary")]
    public async Task<ActionResult<ReputationSummaryDto>> GetReputationSummary(int userId)
    {
        try
        {
            var summary = await _reputationService.GetReputationSummaryAsync(userId);
            return Ok(summary);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    /// <summary>
    /// 获取用户信誉明细
    /// </summary>
    [HttpGet("users/{userId}/reputation/detail")]
    public async Task<ActionResult<ReputationDetailDto>> GetReputationDetail(int userId)
    {
        try
        {
            var detail = await _reputationService.GetReputationDetailAsync(userId);
            return Ok(detail);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }
}

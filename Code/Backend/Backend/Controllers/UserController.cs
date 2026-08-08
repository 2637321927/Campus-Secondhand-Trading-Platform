using Backend.Dtos.User;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

/// <summary>
/// 用户信息查询与个人中心
/// </summary>
[ApiController]
[Route("api/users")]
public class UserController : ControllerBase
{
    private readonly IBaseUserService _userService;

    public UserController(IBaseUserService userService)
    {
        _userService = userService;
    }

    /// <summary>
    /// 根据 ID 获取用户公开信息
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<UserDto>> GetById(int id)
    {
        var user = await _userService.GetByIdAsync(id);
        if (user == null) return NotFound();
        return Ok(user);
    }

    /// <summary>
    /// 获取个人中心资料
    /// </summary>
    [Authorize]
    [HttpGet("me/profile")]
    public async Task<ActionResult<UserProfileDto>> GetMyProfile()
    {
        var userId = int.Parse(User.FindFirst("userId")!.Value);
        var profile = await _userService.GetProfileAsync(userId);
        if (profile == null) return NotFound(new { error = "用户不存在" });
        return Ok(profile);
    }

    /// <summary>
    /// 修改个人中心资料（支持部分更新，仅传入需要修改的字段）
    /// </summary>
    [Authorize]
    [HttpPut("me/profile")]
    public async Task<ActionResult<UserProfileDto>> UpdateMyProfile([FromBody] UpdateProfileDto dto)
    {
        try
        {
            var userId = int.Parse(User.FindFirst("userId")!.Value);
            var profile = await _userService.UpdateProfileAsync(userId, dto);
            if (profile == null) return NotFound(new { error = "用户不存在" });
            return Ok(profile);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// 上传/更换当前用户头像（multipart/form-data）
    /// </summary>
    [Authorize]
    [HttpPost("me/avatar")]
    public async Task<ActionResult<AvatarUploadResponseDto>> UploadAvatar(IFormFile file)
    {
        try
        {
            var userId = int.Parse(User.FindFirst("userId")!.Value);
            var result = await _userService.UploadAvatarAsync(userId, file);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}

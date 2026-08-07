using Backend.Dtos.User;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

/// <summary>
/// 用户信息查询与个人中心管理
/// </summary>
[ApiController]
[Route("api/users")]
public class UserController : ControllerBase
{
    private readonly IBaseUserService _baseUserService;
    private readonly IUserService _userService;

    public UserController(IBaseUserService baseUserService, IUserService userService)
    {
        _baseUserService = baseUserService;
        _userService = userService;
    }

    /// <summary>
    /// 根据 ID 获取用户公开信息（公开接口）
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<UserDto>> GetById(int id)
    {
        var user = await _baseUserService.GetByIdAsync(id);
        if (user == null) return NotFound();
        return Ok(user);
    }

    /// <summary>
    /// 获取当前用户个人中心资料
    /// </summary>
    [Authorize]
    [HttpGet("me/profile")]
    public async Task<ActionResult<UserProfileDto>> GetProfile()
    {
        var userId = int.Parse(User.FindFirst("userId")!.Value);
        var profile = await _userService.GetProfileAsync(userId);
        if (profile == null) return NotFound("用户不存在");
        return Ok(profile);
    }

    /// <summary>
    /// 修改当前用户个人资料（手机号、性别、昵称、简介）
    /// </summary>
    [Authorize]
    [HttpPut("me/profile")]
    public async Task<ActionResult<UserProfileDto>> UpdateProfile([FromBody] UpdateProfileDto dto)
    {
        try
        {
            var userId = int.Parse(User.FindFirst("userId")!.Value);
            var profile = await _userService.UpdateProfileAsync(userId, dto);
            return Ok(profile);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// 上传或更换当前用户头像
    /// </summary>
    [Authorize]
    [HttpPost("me/avatar")]
    public async Task<ActionResult<UserProfileDto>> UpdateAvatar(IFormFile file)
    {
        try
        {
            var userId = int.Parse(User.FindFirst("userId")!.Value);
            var profile = await _userService.UpdateAvatarAsync(userId, file);
            return Ok(profile);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}

using Backend.Dtos.Auth;
using Backend.Dtos.User;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Backend.Controllers;

/// <summary>
/// 账号认证与权限模块
/// </summary>
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IBaseUserService _userService;

    public AuthController(IBaseUserService userService)
    {
        _userService = userService;
    }

    /// <summary>
    /// 普通用户注册
    /// </summary>
    [HttpPost("register")]
    public async Task<ActionResult<UserDto>> Register([FromBody] RegisterDto dto)
    {
        try
        {
            var user = await _userService.RegisterAsync(dto);
            return CreatedAtAction(nameof(Me), null, new { message = "注册成功", user });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
    }

    /// <summary>
    /// 普通用户或管理员登录
    /// </summary>
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginDto dto)
    {
        try
        {
            var result = await _userService.LoginAsync(dto);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return Unauthorized(new { error = ex.Message });
        }
    }

    /// <summary>
    /// 当前账号退出登录
    /// </summary>
    [Authorize]
    [HttpPost("logout")]
    public IActionResult Logout()
    {
        //JWT无状态，服务端无需额外操作，前端丢弃Token即可
        return Ok(new { message = "已退出登录" });
    }

    /// <summary>
    /// 获取当前登录账号身份、角色和基础信息
    /// </summary>
    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<MeResponseDto>> Me()
    {
        var userId = int.Parse(User.FindFirst("userId")!.Value);
        var user = await _userService.GetMeAsync(userId);
        if (user == null)
            return NotFound(new { error = "用户不存在" });
        return Ok(user);
    }

    /// <summary>
    /// 修改当前账号密码
    /// </summary>
    [Authorize]
    [HttpPut("password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
    {
        try
        {
            var userId = int.Parse(User.FindFirst("userId")!.Value);
            await _userService.ChangePasswordAsync(userId, dto);
            return Ok(new { message = "密码修改成功" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// 发起找回密码或重置密码流程
    /// </summary>
    [HttpPost("password/reset-request")]
    public async Task<IActionResult> RequestPasswordReset([FromBody] ResetPasswordRequestDto dto)
    {
        await _userService.RequestPasswordResetAsync(dto);
        //始终返回成功，防止邮箱枚举
        return Ok(new { message = "如果该邮箱已注册，您将收到重置验证码" });
    }

    /// <summary>
    /// 确认重置密码
    /// </summary>
    [HttpPost("password/reset-confirm")]
    public async Task<IActionResult> ConfirmPasswordReset([FromBody] ResetPasswordConfirmDto dto)
    {
        try
        {
            await _userService.ConfirmPasswordResetAsync(dto);
            return Ok(new { message = "密码重置成功，请重新登录" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// 校验当前账号是否具备指定功能访问权限
    /// </summary>
    [Authorize]
    [HttpGet("permission-check")]
    public async Task<IActionResult> CheckPermission([FromQuery] string permission)
    {
        var userId = int.Parse(User.FindFirst("userId")!.Value);
        var hasPermission = await _userService.CheckPermissionAsync(userId, permission);
        return Ok(new { hasPermission, permission });
    }
}

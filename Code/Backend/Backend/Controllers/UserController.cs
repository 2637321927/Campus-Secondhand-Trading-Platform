using Backend.Dtos.User;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IBaseUserService _userService;

    public UserController(IBaseUserService userService)
    {
        _userService = userService;
    }

    /// <summary>
    /// 用户注册（自动创建 base_user + norm_user）
    /// </summary>
    [HttpPost("register")]
    public async Task<ActionResult<UserDto>> Register([FromBody] RegisterDto dto)
    {
        try
        {
            var user = await _userService.RegisterAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = user.UserId }, user);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// 根据 ID 获取用户信息
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<UserDto>> GetById(int id)
    {
        var user = await _userService.GetByIdAsync(id);
        if (user == null) return NotFound();
        return Ok(user);
    }
}

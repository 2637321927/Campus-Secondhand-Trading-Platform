using Backend.Dtos.Payment;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

/// <summary>
/// 支付模块
/// </summary>
[ApiController]
[Route("api")]
public class PaymentController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    /// <summary>
    /// 获取可用支付方式
    /// </summary>
    [HttpGet("payment-methods")]
    public ActionResult<List<PaymentMethodDto>> GetPaymentMethods()
    {
        return Ok(_paymentService.GetPaymentMethods());
    }

    /// <summary>
    /// 发起订单支付
    /// </summary>
    [Authorize]
    [HttpPost("payments")]
    public async Task<ActionResult<PaymentDto>> CreatePayment([FromBody] CreatePaymentDto dto)
    {
        try
        {
            var userId = int.Parse(User.FindFirst("userId")!.Value);
            var payment = await _paymentService.CreatePaymentAsync(userId, dto);
            return CreatedAtAction(nameof(GetPaymentStatus), new { paymentId = payment.PaymentId }, payment);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { error = ex.Message });
        }
    }

    /// <summary>
    /// 查询支付状态
    /// </summary>
    [HttpGet("payments/{paymentId}/status")]
    public async Task<ActionResult<PaymentStatusDto>> GetPaymentStatus(long paymentId)
    {
        try
        {
            var status = await _paymentService.GetPaymentStatusAsync(paymentId);
            return Ok(status);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    /// <summary>
    /// 取消支付
    /// </summary>
    [Authorize]
    [HttpPost("payments/{paymentId}/cancel")]
    public async Task<ActionResult<PaymentDto>> CancelPayment(long paymentId)
    {
        try
        {
            var userId = int.Parse(User.FindFirst("userId")!.Value);
            var payment = await _paymentService.CancelPaymentAsync(paymentId, userId);
            return Ok(payment);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { error = ex.Message });
        }
    }

    /// <summary>
    /// 接收第三方支付回调（模拟）
    /// </summary>
    [HttpPost("payments/{paymentId}/callback")]
    public async Task<ActionResult<PaymentDto>> PaymentCallback(long paymentId, [FromBody] PaymentCallbackDto dto)
    {
        try
        {
            var payment = await _paymentService.HandleCallbackAsync(paymentId, dto);
            return Ok(payment);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}

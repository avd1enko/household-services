using Microsoft.AspNetCore.Mvc;
using PaymentService.Application.DTOs;
using PaymentService.Application.Services;

namespace PaymentService.API.Controllers;

[ApiController]
[Route("api/payment")]
public class PaymentController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    [HttpPost("pay")]
    public async Task<ActionResult<PaymentStatusResponse>> PayAsync(CreatePaymentRequest request)
    {
        PaymentStatusResponse response = await _paymentService.PayAsync(request);

        if (response.IsPaid)
            return Ok(response);

        return BadRequest(response);
    }
}

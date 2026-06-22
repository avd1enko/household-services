using HouseholdServices.Application.DTOs.Payment;
using HouseholdServices.Application.Services.Payment;
using Microsoft.AspNetCore.Mvc;

namespace HouseholdServices.API.Controllers;

[ApiController]
[Route("api/payment-test")]
public class PaymentTestController : ControllerBase
{
    private readonly IPaymentTestService _paymentTestService;

    public PaymentTestController(IPaymentTestService paymentTestService)
    {
        _paymentTestService = paymentTestService;
    }

    [HttpPost("pay")]
    public async Task<ActionResult<PaymentStatusResponse>> PayAsync()
    {
        PaymentStatusResponse response = await _paymentTestService.PayTestOrderAsync();
        return Ok(response);
    }
}

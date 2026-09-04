using delice_api.DTOs;
using delice_api.Entities;
using delice_api.Services;
using Microsoft.AspNetCore.Mvc;

namespace delice_api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentController : ControllerBase
{
    private readonly PaymentService _paymentService;

    public PaymentController(PaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    [HttpPost]
    public async Task<ActionResult<CartDto>> CreateOrUpdatePaymentIntent(Cart cart)
    {
        if (cart is null)
            return NoContent();

        var intent = await _paymentService.CreateOrUpdatePaymentIntentAsync(cart);

        if (intent is null)
            return BadRequest(new ProblemDetails { Title = "Problem creating payment intent" });

        cart.PaymentIntentId ??= intent.Id;
        cart.ClientSecret ??= intent.ClientSecret;

        return new CartDto
        {
            TableNumber = cart.TableNumber,
            PaymentIntentId = cart.PaymentIntentId,
            ClientSecret = cart.ClientSecret,
            Products = cart.Products.Select(p => new CartProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                ImageUrl = p.ImageUrl,
                Type = p.Type,
                Quantity = p.Quantity
            }).ToList()
        };
    }
}

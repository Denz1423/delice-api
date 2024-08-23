using Microsoft.AspNetCore.Mvc;
using delice_api.DTOs;
using delice_api.Entities;
using delice_api.Services;

namespace delice_api.Controllers
{
    [ApiController]
    [Route("[controller]")]
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
            if (cart == null)
                return NoContent();

            var intent = await _paymentService.CreateOrUpdatePaymentIntent(cart);

            if (intent == null)
                return BadRequest(new ProblemDetails { Title = "Problem creating payment intent" });

            cart.PaymentIntentId ??= intent.Id;
            cart.ClientSecret ??= intent.ClientSecret;

            return new CartDto
            {
                TableNumber = cart.TableNumber,
                PaymentIntentId = cart.PaymentIntentId,
                ClientSecret = cart.ClientSecret,
                Products = cart
                    .Products.Select(product => new CartProductDto
                    {
                        Id = product.Id,
                        Name = product.Name,
                        Price = product.Price,
                        ImageUrl = product.ImageUrl,
                        Type = product.Type,
                        Quantity = product.Quantity
                    })
                    .ToList()
            };
        }
    }
}

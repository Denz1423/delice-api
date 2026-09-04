using delice_api.DTOs;
using delice_api.Entities;
using delice_api.Services;
using Microsoft.AspNetCore.Mvc;

namespace delice_api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpGet]
    public async Task<ActionResult<List<OrderDto>>> GetAllOrders()
    {
        var orders = await _orderService.GetAllAsync();
        return Ok(orders);
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder(Cart cart)
    {
        if (cart.Products is null || cart.Products.Count == 0)
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid Cart",
                Detail = "The cart is empty. Please add items before creating an order."
            });

        var orderId = await _orderService.CreateAsync(cart);
        return Ok(new { OrderId = orderId });
    }
}

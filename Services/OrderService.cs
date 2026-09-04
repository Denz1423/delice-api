using delice_api.DTOs;
using delice_api.Entities;
using delice_api.Entities.Order;
using delice_api.Repositories;

namespace delice_api.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;

    public OrderService(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<List<OrderDto>> GetAllAsync()
    {
        var orders = await _orderRepository.GetAllAsync();

        return orders.Select(o => new OrderDto
        {
            Id = o.Id,
            TableNumber = o.TableNumber,
            OrderDate = o.OrderDate,
            OrderItems = o.OrderItems.Select(i => new OrderItemDto
            {
                Id = i.Id,
                Name = i.Name,
                Type = i.Type,
                Price = i.Price,
                ImageUrl = i.ImageUrl,
                Quantity = i.Quantity
            }).ToList(),
            SubTotal = o.SubTotal,
            OrderStatus = o.OrderStatus,
            PaymentStatus = o.PaymentStatus,
            PaymentIntentId = o.PaymentIntentId
        }).ToList();
    }

    public async Task<string> CreateAsync(Cart cart)
    {
        var orderId = Guid.NewGuid().ToString();
        var subTotal = cart.Products.Sum(p => p.Price * p.Quantity);

        var order = new Order
        {
            Id = orderId,
            TableNumber = cart.TableNumber,
            OrderDate = DateTime.UtcNow,
            OrderItems = cart.Products,
            SubTotal = subTotal,
            OrderStatus = OrderStatus.Pending,
            // Start as Pending — PaymentStatus.Success is set via Stripe webhook
            PaymentStatus = PaymentStatus.Pending,
            PaymentIntentId = cart.PaymentIntentId
        };

        await _orderRepository.CreateAsync(order);
        return orderId;
    }
}

using delice_api.Entities.Order;

namespace delice_api.Repositories;

public interface IOrderRepository
{
    Task<List<Order>> GetAllAsync();
    Task CreateAsync(Order order);
    Task UpdatePaymentStatusAsync(string orderId, PaymentStatus status);
}

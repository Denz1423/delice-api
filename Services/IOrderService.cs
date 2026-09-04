using delice_api.DTOs;
using delice_api.Entities;

namespace delice_api.Services;

public interface IOrderService
{
    Task<List<OrderDto>> GetAllAsync();
    Task<string> CreateAsync(Cart cart);
}

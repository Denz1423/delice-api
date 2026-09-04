using System.ComponentModel.DataAnnotations;
using delice_api.Entities.Order;

namespace delice_api.DTOs;

public class OrderDto
{
    public string Id { get; set; } = string.Empty;

    [Required]
    public int TableNumber { get; set; }

    public DateTime OrderDate { get; set; }
    public List<OrderItemDto> OrderItems { get; set; } = [];
    public double SubTotal { get; set; }
    public OrderStatus OrderStatus { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public string? PaymentIntentId { get; set; }
}

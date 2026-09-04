using System.ComponentModel.DataAnnotations;
using delice_api.Entities;

namespace delice_api.Entities.Order;

public class Order
{
    public string Id { get; set; } = string.Empty;

    [Required]
    public int TableNumber { get; set; }

    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    public List<CartProduct> OrderItems { get; set; } = [];
    public double SubTotal { get; set; }
    public OrderStatus OrderStatus { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public string? PaymentIntentId { get; set; }
}

using System.ComponentModel.DataAnnotations;

namespace delice_api.Entities.Order
{
    public class Order
    {
        public string Id { get; set; }

        [Required]
        public int TableNumber { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public List<CartProduct> OrderItems { get; set; }
        public double SubTotal { get; set; }
        public OrderStatus OrderStatus { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public string PaymentIntentId { get; set; }
    }
}

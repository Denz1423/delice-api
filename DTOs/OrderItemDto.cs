namespace delice_api.DTOs
{
    public class OrderItemDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public double Price { get; set; }
        public string Type { get; set; }
        public int Quantity { get; set; }
    }
}

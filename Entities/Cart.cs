namespace delice_api.Entities
{
    public class Cart
    {
        public int TableNumber { get; set; }
        public List<CartProduct> Products { get; set; }
        public string PaymentIntentId { get; set; }
        public string ClientSecret { get; set; }
    }
}

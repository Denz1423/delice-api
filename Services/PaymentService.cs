using delice_api.Entities;
using Stripe;

namespace delice_api.Services
{
    public class PaymentService
    {
        private readonly IConfiguration _config;

        public PaymentService(IConfiguration config)
        {
            _config = config;
        }

        public async Task<PaymentIntent> CreateOrUpdatePaymentIntent(Cart cart)
        {
            StripeConfiguration.ApiKey = _config["StripeSettings:SecretKey"];

            var paymentIntentService = new PaymentIntentService();

            var paymentIntent = new PaymentIntent();
            var subtotal = cart.Products.Sum(item => item.Price * item.Quantity) * 100;

            if (string.IsNullOrEmpty(cart.PaymentIntentId))
            {
                var options = new PaymentIntentCreateOptions
                {
                    Amount = (long)subtotal,
                    Currency = "nzd",
                };
                paymentIntent = await paymentIntentService.CreateAsync(options);
            }
            else
            {
                var options = new PaymentIntentUpdateOptions { Amount = (long)subtotal, };
                await paymentIntentService.UpdateAsync(cart.PaymentIntentId, options);
            }
            return paymentIntent;
        }
    }
}

using delice_api.Entities;
using Stripe;

namespace delice_api.Services;

public class PaymentService
{
    // StripeConfiguration.ApiKey is set once at startup in Program.cs
    private readonly PaymentIntentService _intentService = new();

    public async Task<PaymentIntent?> CreateOrUpdatePaymentIntentAsync(Cart cart)
    {
        var amount = (long)(cart.Products.Sum(p => p.Price * p.Quantity) * 100);

        if (string.IsNullOrEmpty(cart.PaymentIntentId))
        {
            var options = new PaymentIntentCreateOptions
            {
                Amount = amount,
                Currency = "nzd"
            };
            return await _intentService.CreateAsync(options);
        }

        var updateOptions = new PaymentIntentUpdateOptions { Amount = amount };
        return await _intentService.UpdateAsync(cart.PaymentIntentId, updateOptions);
    }
}

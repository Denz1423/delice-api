using delice_api.Entities.Order;
using delice_api.Repositories;
using Microsoft.AspNetCore.Mvc;
using Stripe;

namespace delice_api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WebhooksController : ControllerBase
{
    private readonly IOrderRepository _orderRepository;
    private readonly IConfiguration _config;
    private readonly ILogger<WebhooksController> _logger;

    public WebhooksController(
        IOrderRepository orderRepository,
        IConfiguration config,
        ILogger<WebhooksController> logger)
    {
        _orderRepository = orderRepository;
        _config = config;
        _logger = logger;
    }

    [HttpPost("stripe")]
    public async Task<IActionResult> HandleStripeEvent()
    {
        var json = await new StreamReader(Request.Body).ReadToEndAsync();
        var webhookSecret = _config["StripeSettings:WebhookSecret"];

        Event stripeEvent;
        try
        {
            stripeEvent = EventUtility.ConstructEvent(
                json,
                Request.Headers["Stripe-Signature"],
                webhookSecret
            );
        }
        catch (StripeException ex)
        {
            _logger.LogWarning("Stripe webhook signature verification failed: {Message}", ex.Message);
            return BadRequest();
        }

        switch (stripeEvent.Type)
        {
            case "payment_intent.succeeded":
                await HandlePaymentIntentSucceeded(stripeEvent.Data.Object as PaymentIntent);
                break;

            case "payment_intent.payment_failed":
                await HandlePaymentIntentFailed(stripeEvent.Data.Object as PaymentIntent);
                break;
        }

        return Ok();
    }

    private async Task HandlePaymentIntentSucceeded(PaymentIntent? intent)
    {
        if (intent is null) return;

        _logger.LogInformation("PaymentIntent succeeded: {Id}", intent.Id);

        // Find order by PaymentIntentId and mark it paid.
        // Note: this currently requires a scan because there is no GSI on PaymentIntentId.
        // Consider adding a GSI on PaymentIntentId to avoid the full table scan.
        var orders = await _orderRepository.GetAllAsync();
        var order = orders.FirstOrDefault(o => o.PaymentIntentId == intent.Id);

        if (order is not null)
            await _orderRepository.UpdatePaymentStatusAsync(order.Id, PaymentStatus.Success);
    }

    private Task HandlePaymentIntentFailed(PaymentIntent? intent)
    {
        if (intent is null) return Task.CompletedTask;

        _logger.LogWarning("PaymentIntent failed: {Id}", intent.Id);
        return Task.CompletedTask;
    }
}

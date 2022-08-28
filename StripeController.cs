using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Stripe;

namespace StripeDemoOKR
{
    [Route("api/")]
    [ApiController]
    public class StripeController : Controller
    {
        private readonly IConfiguration _configuration;

        public StripeController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create(PaymentRequest request)
        {
            var paymentService = new PaymentIntentService();
            var payment = await paymentService.CreateAsync(new PaymentIntentCreateOptions
            {
                Amount = request.Amount,
                Currency = request.Currency ?? "usd",
                AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
                {
                    Enabled = true
                }
            });

            return new OkObjectResult(new {ClientSecret = payment.ClientSecret});
        }

        [HttpPost("StripeWebhook")]
        public async Task<IActionResult> StripeWebhook()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

            var webhookSecret = _configuration.GetSection("Stripe")["WebhookEndpointSecret"];

            try
            {
                var stripeEvent = EventUtility.ParseEvent(json);
                var signatureHeader = Request.Headers["Stripe-Signature"];

                stripeEvent = EventUtility.ConstructEvent(json, signatureHeader, webhookSecret);

                switch (stripeEvent.Type)
                {
                    case Events.PaymentIntentSucceeded:
                        var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
                        Console.WriteLine(
                            $"Event handled: PaymentIntentSucceeded {paymentIntent.Amount} {paymentIntent.Currency}");
                        break;
                    case Events.PaymentMethodAttached:
                        var paymentMethod = stripeEvent.Data.Object as PaymentMethod;
                        Console.WriteLine($"Event handled: PaymentMethodAttached {paymentMethod.Type}");
                        break;
                    default:
                        Console.WriteLine("Unhandled event type: {0}", stripeEvent.Type);
                        break;
                }

                return new OkResult();
            }
            catch (StripeException e)
            {
                Console.WriteLine(e);
                return new BadRequestResult();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return new StatusCodeResult(500);
            }
        }
    }
}
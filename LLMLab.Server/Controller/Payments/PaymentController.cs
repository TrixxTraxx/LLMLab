using System.Security.Claims;
using Azure;
using Hangfire;
using LLMLab.Server.Configuration;
using LLMLab.Server.Data;
using LLMLab.Server.Jobs.Payments;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;
using Stripe.Entitlements;
using Stripe.Forwarding;

namespace LLMLab.Server.Controller;

[ApiController]
public class PaymentController(
    ApplicationDbContext dbContext,
    IOptions<Appsettings> appSettings
) : ControllerBase
{
    [HttpPost("/create-checkout-session/{credits}")]
    public ActionResult CreateCheckout(int credits)
    {
        var userId = Request.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

        string apiKey = appSettings.Value.StripeSecretKey;
        var client = new StripeClient(apiKey);
        var domain = appSettings.Value.ClientUrl;

        var priceOptions = new PriceListOptions
        {
            LookupKeys = new List<string> {
                "credits-" + credits
            }
        };
        var priceService = new PriceService(client);
        StripeList<Price> prices = priceService.List(priceOptions);

        var options = new SessionCreateOptions
        {
            LineItems = new List<SessionLineItemOptions>
            {
                new SessionLineItemOptions
                {
                    Price = prices.Data[0].Id,
                    Quantity = 1,
                },
            },
            Mode = "payment",
            SuccessUrl = domain + "/CheckoutSuccess?session_id={CHECKOUT_SESSION_ID}",
            CancelUrl = domain + "/CheckoutCanceled",
            Metadata = new Dictionary<string, string>
            {
                { "user_id", userId},
                { "credits", credits.ToString() }
            },
        };
        var service = new SessionService(client);
        Session session = service.Create(options);

        Response.Headers.Add("Location", session.Url);
        return new StatusCodeResult(303);
    }
    
    [HttpPost("/create-portal-session")]
    public ActionResult Create()
    {
        string apiKey = appSettings.Value.StripeSecretKey;
        var client = new StripeClient(apiKey);
        var domain = appSettings.Value.ClientUrl;

        // For demonstration purposes, we're using the Checkout session to retrieve the customer ID.
        // Typically this is stored alongside the authenticated user in your database.
        var checkoutService = new SessionService(client);
        var checkoutSession = checkoutService.Get(Request.Form["session_id"]);

        // This is the URL to which your customer will return after
        // they're done managing billing in the Customer Portal.
        var returnUrl = domain;

        var options = new Stripe.BillingPortal.SessionCreateOptions
        {
            Customer = checkoutSession.CustomerId,
            ReturnUrl = returnUrl,
        };
        var service = new Stripe.BillingPortal.SessionService(client);
        var session = service.Create(options);

        Response.Headers.Add("Location", session.Url);
        return new StatusCodeResult(303);
    }
    
    [HttpPost("/webhook")]
    public async Task<IActionResult> Index()
    {
        var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
        string endpointSecret = appSettings.Value.StripeWebhookSecret;
        try
        {
            var stripeEvent = EventUtility.ConstructEvent(json,
                Request.Headers["Stripe-Signature"], endpointSecret);

            // Handle the event
            if (stripeEvent.Type == EventTypes.CheckoutSessionCompleted)
            {
                var session = stripeEvent.Data.Object as Session;
                // Fulfill the purchase.
                await FulfillOrder(session);
            }
            else
            {
                Console.WriteLine("Unhandled event type: {0}", stripeEvent.Type);
            }

            return Ok();
        }
        catch (StripeException e)
        {
            Console.WriteLine("Error: {0}", e.Message);
            return BadRequest();
        }
    }
    
    /// <summary>
    /// Contains the business logic to fulfill the order after a successful payment.
    /// </summary>
    private async Task FulfillOrder(Session session)
    {
        BackgroundJob.Enqueue<AddCreditsJob>(x => x.Execute(
            session.Id,
            session.Metadata["user_id"],
            int.Parse(session.Metadata["credits"])
        ));
    }
}
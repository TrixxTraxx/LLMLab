using Hangfire;
using LLMLab.Server.Configuration;
using LLMLab.Server.Data;
using LLMLab.Server.Data.Payments;
using Microsoft.Extensions.Options;

namespace LLMLab.Server.Jobs.Payments;

[AutomaticRetry(Attempts = 10)]
public class AddCreditsJob(
    ApplicationDbContext dbContext,
    IOptions<Appsettings> appSettings
)
{
    public async Task Execute(string sessionId, string userId, int credits)
    {
        var stripeClient = new Stripe.StripeClient(appSettings.Value.StripeSecretKey);
        var sessionService = new Stripe.Checkout.SessionService(stripeClient);
        // Retrieve the session from Stripe
        var session = await sessionService.GetAsync(sessionId);
        
        // Check that the payment status is "paid"
        if (session.PaymentStatus == "paid")
        {
            Console.WriteLine($"Payment successful. Fulfilling order for user: {userId}, adding {credits} credits.");
            
            if(!appSettings.Value.CreditsPerPrice.ContainsKey(credits.ToString()))
            {
                // TODO: Handle case where user does not exist in CreditsPerPrice
                throw new NotImplementedException();
            }
            // Update the user's credits in the database
            var receivedCredits = appSettings.Value.CreditsPerPrice[credits.ToString()];
            var user = await dbContext.Users.FindAsync(userId);
            if (user != null)
            {
                var paymentTransaction = new PaymentTransaction
                {
                    TotalBoughtCreditsBefore = user.TotallyBoughtCredits,
                    BoughtCredits = receivedCredits,
                    SessionId = sessionId,
                    TimestampUtc = DateTime.UtcNow
                };
                
                user.TotallyBoughtCredits += receivedCredits;
                
                paymentTransaction.TotalBoughtCreditsAfter = receivedCredits;
                dbContext.PaymentTransactions.Add(paymentTransaction);
                dbContext.Users.Update(user);
                await dbContext.SaveChangesAsync();
                Console.WriteLine($"User {userId} now has bought {user.TotallyBoughtCredits} credits and {user.AvailableCredits} credits available in total.");
            }
            else
            {
                Console.WriteLine($"Error: User with ID {userId} not found.");
            }
        }
        else
        {
            throw new InvalidOperationException($"Payment for session {sessionId} was not successful. Status: {session.PaymentStatus}");
        }
    }
}
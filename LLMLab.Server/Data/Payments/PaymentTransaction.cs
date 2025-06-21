namespace LLMLab.Server.Data.Payments;

public class PaymentTransaction
{
    public int Id { get; set; }
    
    public int TotalBoughtCreditsBefore { get; set; } = 0;
    public int TotalBoughtCreditsAfter { get; set; } = 0;
    public int BoughtCredits { get; set; } = 0;
    
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;
    
    public string SessionId { get; set; } = string.Empty;
    public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;
}
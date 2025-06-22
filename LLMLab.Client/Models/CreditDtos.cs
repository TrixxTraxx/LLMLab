namespace LLMLab.Client.Models;

// DTOs for Credit Service
public class CreditTransaction
{
    public int Id { get; set; }
    public int Amount { get; set; }
    public string Type { get; set; } = string.Empty; // "Purchase", "Deduction", "Bonus"
    public decimal? Price { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string? StripeSessionId { get; set; }
}

public class CreditDeductRequest
{
    public int Amount { get; set; }
    public string Reason { get; set; } = string.Empty;
} 
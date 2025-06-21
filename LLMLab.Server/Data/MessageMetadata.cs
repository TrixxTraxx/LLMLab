namespace LLMLab.Server.Data;

public class MessageMetadata 
{
    public int Id { get; set; }
    
    public int MessageId { get; set; }
    public Message Message { get; set; } = null!;
    
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;
    
    public int InputTokens { get; set; }
    public int ThinkingTokens { get; set; }
    public int OutputTokens { get; set; }
    
    public double InputTokenCost { get; set; } = 0.0;
    public double ThinkingTokenCost { get; set; } = 0.0;
    public double OutputTokenCost { get; set; } = 0.0;
    
    public bool HasError { get; set; } = false;
    public string ErrorMessage { get; set; }
    
    public string ModelName { get; set; } = string.Empty;
    public string ModelProvider { get; set; } = string.Empty;
    public string ModelId { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool UsedUserApiKey { get; set; } = false;
    
    public double ResponseTimeSeconds { get; set; } = 0.0;
    public double TotalInputTokenCost { get; set; } = 0.0;
    public double TotalThinkingTokenCost { get; set; } = 0.0;
    public double TotalOutputTokenCost { get; set; } = 0.0;
    
    public double TotalCost { get; set; } = 0.0;
}
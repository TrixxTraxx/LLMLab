namespace LLMLab.Server.Service.Models;

public class ChatModelResponse
{
    public int InputTokens { get; set; }
    public int ThinkingTokens { get; set; }
    public int OutputTokens { get; set; }
    public bool IsError { get; set; } = false;
    public string ErrorMessage { get; set; } = string.Empty;
}
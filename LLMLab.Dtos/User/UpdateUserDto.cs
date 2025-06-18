namespace LLMLab.Dtos.User;

public class UpdateUserDto
{
    public string DisplayName { get; set; }
    
    //System Prompt Settings
    public string UserRole { get; set; } = "";
    public string UserSystemPrompt { get; set; } = string.Empty;
    public string[] UserSystemTraits { get; set; } = new string[0];
}
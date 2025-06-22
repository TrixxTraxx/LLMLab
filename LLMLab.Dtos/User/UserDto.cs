namespace LLMLab.Dtos.User;

public class UserDto
{
    public string Id { get; set; }
    public string UserName { get; set; }
    public string Email { get; set; }
    public string ProfilePictureUrl { get; set; }
    public string DisplayName { get; set; }
    
    public int AvailableCredits { get; set; }
    
    public string UserRole { get; set; } = "";
    public string UserSystemPrompt { get; set; } = string.Empty;
    public string[] UserSystemTraits { get; set; } = new string[0];
}
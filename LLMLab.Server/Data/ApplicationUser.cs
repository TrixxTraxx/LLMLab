using Microsoft.AspNetCore.Identity;

namespace LLMLab.Server.Data;

// Add profile data for application users by adding properties to the ApplicationUser class
public class ApplicationUser : IdentityUser
{
    public string ProfilePictureUrl { get; set; } = "";
    
    public int ThreadVersion { get; set; } = 2;
    public string DisplayName { get; set; }
    
    //System Prompt Settings
    public string UserRole { get; set; } = "";
    public string UserSystemPrompt { get; set; } = string.Empty;
    public string[] UserSystemTraits { get; set; } = new string[0];
    
}


using System.Globalization;
using LLMLab.Server.Configuration;
using LLMLab.Server.Data;
using Microsoft.Extensions.Options;

namespace LLMLab.Server.Service;

public class SystemPromptService(
    IOptions<Appsettings> appSettings
)
{
    public string GetSystemprompt(AiModel aiModel, ApplicationUser user)
    {
        var systemPromptTemplate = appSettings.Value.SystemPromptTemplate;
        
        // Replace placeholders with actual values
        var processedPrompt = systemPromptTemplate
            .Replace("{{Current_Datetime_UTC}}", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC"))
            .Replace("{{Current_User_Name}}", GetUserDisplayName(user))
            .Replace("{{Current_AI_Model}}", aiModel.Name)
            .Replace("{{Current_User_Role}}", user.UserRole ?? string.Empty)
            .Replace("{{Current_User_System_Traits}}", FormatUserTraits(user.UserSystemTraits))
            .Replace("{{Current_User_System_Prompt}}", user.UserSystemPrompt ?? string.Empty);
        
        return processedPrompt;
    }
    
    private static string GetUserDisplayName(ApplicationUser user)
    {
        if (!string.IsNullOrWhiteSpace(user.DisplayName))
            return user.DisplayName;
        
        if (!string.IsNullOrWhiteSpace(user.UserName))
            return user.UserName;
            
        return "User";
    }
    
    private static string FormatUserTraits(string[] traits)
    {
        if (traits == null || traits.Length == 0)
            return string.Empty;
            
        return string.Join(", ", traits.Where(t => !string.IsNullOrWhiteSpace(t)));
    }
}
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
        
        return systemPromptTemplate;
    }
}
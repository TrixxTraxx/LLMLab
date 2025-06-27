using Hangfire.Dashboard;

namespace LLMLab.Server.Service;

public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();
#if DEBUG
        return true;
#endif
        return httpContext.User.IsInRole("Admin");
    }
}
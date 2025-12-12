using Serilog.Context;
using System.Security.Claims;

namespace MyApp.Api.Middleware;

public class SerilogUserEnricherMiddleware
{
    private readonly RequestDelegate _next;

    public SerilogUserEnricherMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        string userName = "Anonymous";
        string userId = "N/A";
        string role = "None";

        if (context.User is { Identity.IsAuthenticated: true } user)
        {
            // 優先順序：Name → "name" → "unique_name" → "userName" → "sub"
            userName =
                user.FindFirst(ClaimTypes.Name)?.Value ??
                user.FindFirst("name")?.Value ??
                user.FindFirst("unique_name")?.Value ??
                user.FindFirst("userName")?.Value ??
                user.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                "UnknownUser";

            userId =
                user.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                user.FindFirst("sub")?.Value ??
                "N/A";

            // Role：支援 ClaimTypes.Role / "role"
            role =
                user.FindFirst(ClaimTypes.Role)?.Value ??
                user.FindFirst("role")?.Value ??
                "None";
        }

        using (LogContext.PushProperty("UserName", userName))
        using (LogContext.PushProperty("UserId", userId))
        using (LogContext.PushProperty("UserRole", role))
        using (LogContext.PushProperty("TraceId", context.TraceIdentifier))
        {
            await _next(context);
        }
    }
}

using MyApp.Domain;
using System.Security.Claims;

namespace MyApp.Api.Services;

public class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? UserName
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null || !user.Identity?.IsAuthenticated == true)
                return null;

            // Name = "sub" 或 "name" or "unique_name"，我們用 NameIdentifier / Name 優先
            return user.FindFirst(ClaimTypes.Name)?.Value
                ?? user.Identity?.Name;
        }
    }
}

using System.Security.Claims;

namespace Mimir.Api.Security;

public static class HttpContextExtensions
{
    public static string GetUsername(this HttpContext httpContext)
    {
        if (httpContext == null)
            throw new ArgumentNullException(nameof(httpContext));
        var user = httpContext.User;
        if (user.Identity?.IsAuthenticated != true)
            throw new InvalidOperationException("User is not authenticated");
        var name = user.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(name))
            throw new InvalidOperationException("Username is not available");

        return name;
    }
}
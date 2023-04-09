using System.Security.Claims;
using Mimir.Application.Security;

namespace Mimir.Api.Security;

public class HttpUserIdentityProvider : IUserIdentityProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpUserIdentityProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string GetUsername()
    {
        if (_httpContextAccessor.HttpContext == null)
            throw new InvalidOperationException("HttpContext is not available");
        var user = _httpContextAccessor.HttpContext.User;
        if (user.Identity?.IsAuthenticated != true)
            throw new InvalidOperationException("User is not authenticated");
        var name = user.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(name))
            throw new InvalidOperationException("Username is not available");

        return name;
    }
}
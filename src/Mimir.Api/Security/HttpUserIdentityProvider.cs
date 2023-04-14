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
        return _httpContextAccessor.HttpContext.GetUsername();
    }
}
using FastEndpoints;

namespace Mimir.Api.Security;

public static class EndpointSecurityExtensions
{
    public static void RequireChatGptUser(this IEndpoint endpoint)
    {
        endpoint.Definition.Policies("ChatGptUserOnly");
    }
}
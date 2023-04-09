namespace Mimir.Application.Security;

public interface IUserIdentityProvider
{
    string GetUsername();
}
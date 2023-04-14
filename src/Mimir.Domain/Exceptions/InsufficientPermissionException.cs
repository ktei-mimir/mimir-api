namespace Mimir.Domain.Exceptions;

public class InsufficientPermissionException : DomainException
{
    public InsufficientPermissionException(string message) : base(message)
    {
    }
}
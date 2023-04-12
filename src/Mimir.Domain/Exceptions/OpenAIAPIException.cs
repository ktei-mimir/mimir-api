namespace Mimir.Domain.Exceptions;

public class OpenAIAPIException : DomainException
{
    public OpenAIAPIException(string message) : base(message)
    {
    }
}
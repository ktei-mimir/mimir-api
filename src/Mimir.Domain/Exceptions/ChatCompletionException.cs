namespace Mimir.Domain.Exceptions;

public class ChatCompletionException : DomainException
{
    public ChatCompletionException(string message) : base(message)
    {
    }
}
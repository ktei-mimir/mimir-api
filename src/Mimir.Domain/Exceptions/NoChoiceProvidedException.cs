namespace Mimir.Domain.Exceptions;

public class NoChoiceProvidedException : DomainException
{
    public NoChoiceProvidedException() : base("No choice provided from ChatGPT")
    {

    }
}
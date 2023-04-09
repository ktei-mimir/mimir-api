using Mimir.Domain.Helpers;

namespace Mimir.Application.OpenAI;

public readonly record struct GptMessage(string Role, string Content)
{
    public override string ToString() => Content.TakeMax(10);
}
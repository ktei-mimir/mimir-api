using Mimir.Domain.Helpers;

namespace Mimir.Application.ChatGpt;

public readonly record struct GptMessage(string Role, string Content)
{
    public override string ToString() => Content.TakeMax(10);
}
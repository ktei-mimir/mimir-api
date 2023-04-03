namespace Mimir.Application.ChatGpt;

public static class CommandBuilder
{
    public static string Summarize(string text)
    {
        return $"summary this to a title: {text}";
    }
}
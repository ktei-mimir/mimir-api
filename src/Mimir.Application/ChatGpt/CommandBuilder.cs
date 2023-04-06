namespace Mimir.Application.ChatGpt;

public static class CommandBuilder
{
    public static string Summarize(string text)
    {
        return $"summary the following text to a title within 15 words: {text}";
    }
}
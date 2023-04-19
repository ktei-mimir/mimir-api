namespace Mimir.Application.OpenAI;

public static class CommandBuilder
{
    public static string Summarize(string text)
    {
        return $"summarize the following text to a title within 6 words: {text}";
    }
}
namespace Mimir.Application.OpenAI;

public class Completion
{
    public Usage Usage { get; set; } = new();

    public List<CompletionChoice> Choices { get; set; } = new();
}
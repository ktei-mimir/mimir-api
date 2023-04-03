namespace Mimir.Application.ChatGpt;

public class Completion
{
    public string Id { get; set; } = string.Empty;

    public string Object { get; set; } = string.Empty;
    
    public long Created { get; set; }

    public string Model { get; set; } = string.Empty;

    public Usage Usage { get; set; } = new();

    public List<CompletionChoice> Choices { get; set; } = new();
}
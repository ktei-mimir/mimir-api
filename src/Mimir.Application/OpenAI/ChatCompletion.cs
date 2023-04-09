namespace Mimir.Application.OpenAI;

public class ChatCompletion
{
    public string Id { get; set; } = string.Empty;

    public string Object { get; set; } = string.Empty;
    
    public string Model { get; set; } = string.Empty;

    public Usage Usage { get; set; } = new();

    public List<ChatCompletionChoice> Choices { get; set; } = new();
}
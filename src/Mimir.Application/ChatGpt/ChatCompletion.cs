namespace Mimir.Application.ChatGpt;

public class ChatCompletion
{
    public string Id { get; set; } = string.Empty;

    public string Object { get; set; } = string.Empty;
    
    public int Created { get; set; }

    public string Model { get; set; } = string.Empty;

    public Usage Usage { get; set; } = new();

    public List<Choice> Choices { get; set; } = new();
}
namespace Mimir.Application.OpenAI;

public class ChatCompletion
{
    public Usage Usage { get; set; } = new();

    public List<ChatCompletionChoice> Choices { get; set; } = new();
}
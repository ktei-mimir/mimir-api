namespace Mimir.Application.ChatGpt;

public class CreateChatCompletionRequest
{
    public string Model { get; set; } = "gpt-3.5-turbo";
    public List<Message> Messages { get; set; } = new();
}
namespace Mimir.Application.ChatGpt;

public class CreateChatCompletionRequest
{
    public string Model { get; set; } = AIModels.Gpt3Turbo;
    public List<Message> Messages { get; set; } = new();
}
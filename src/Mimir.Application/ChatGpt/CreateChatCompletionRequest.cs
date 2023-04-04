namespace Mimir.Application.ChatGpt;

public class CreateChatCompletionRequest
{
    public string Model { get; set; } = OpenApiModels.Gpt3Turbo;
    public List<Message> Messages { get; set; } = new();
}
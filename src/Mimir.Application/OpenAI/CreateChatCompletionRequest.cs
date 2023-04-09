namespace Mimir.Application.OpenAI;

public class CreateChatCompletionRequest
{
    public string Model { get; set; } = AIModels.Gpt3Turbo;
    public List<GptMessage> Messages { get; set; } = new();
}
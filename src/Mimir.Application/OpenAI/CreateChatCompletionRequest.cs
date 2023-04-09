namespace Mimir.Application.OpenAI;

public class CreateChatCompletionRequest
{
    public List<GptMessage> Messages { get; set; } = new();
}
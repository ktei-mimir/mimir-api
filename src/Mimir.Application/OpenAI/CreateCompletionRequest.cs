
namespace Mimir.Application.OpenAI;

public class CreateCompletionRequest
{
    public string Prompt { get; set; }

    public int MaxTokens { get; set; }
}
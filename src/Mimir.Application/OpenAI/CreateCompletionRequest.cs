using System.Text.Json.Serialization;

namespace Mimir.Application.OpenAI;

public class CreateCompletionRequest
{
    public string Model { get; set; } = AIModels.TextDavinci003;

    public string Prompt { get; set; }

    [JsonPropertyName("max_tokens")]
    public int MaxTokens { get; set; }
}
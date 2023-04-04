using System.Text.Json.Serialization;

namespace Mimir.Application.ChatGpt;

public class CreateCompletionRequest
{
    public string Model { get; set; } = OpenApiModels.TextDavinci003;

    public string Prompt { get; set; }

    [JsonPropertyName("max_tokens")]
    public int MaxTokens { get; set; }
}
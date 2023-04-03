using System.Text.Json.Serialization;

namespace Mimir.Application.ChatGpt;

public class Usage
{
    [JsonPropertyName("prompt_tokens")]
    public int PromptTokens { get; set; }
    
    [JsonPropertyName("completion_tokens")]
    public int CompletionTokens { get; set; }
    
    [JsonPropertyName("total_tokens")]
    public int TotalTokens { get; set; }
    
    public Usage Add(Usage usage)
    {
        return new Usage
        {
            PromptTokens = PromptTokens + usage.PromptTokens,
            CompletionTokens = CompletionTokens + usage.CompletionTokens,
            TotalTokens = TotalTokens + usage.TotalTokens,
        };
    }
}
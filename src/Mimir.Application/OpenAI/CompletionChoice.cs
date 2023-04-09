using System.Text.Json.Serialization;

namespace Mimir.Application.OpenAI;

public class CompletionChoice
{
    public string Text { get; set; }
    
    [JsonPropertyName("finish_reason")]
    public string FinishReason { get; set; } = string.Empty;
    
    public int Index { get; set; }
}
using System.Text.Json.Serialization;

namespace Mimir.Application.ChatGpt;

public class CompletionChoice
{
    public string Text { get; set; }
    
    [JsonPropertyName("finish_reason")]
    public string FinishReason { get; set; } = string.Empty;
    
    public int Index { get; set; }
}
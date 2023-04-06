using System.Text.Json.Serialization;

namespace Mimir.Application.ChatGpt;

public class ChatCompletionChoice
{
    public GptMessage Message { get; set; }
    
    [JsonPropertyName("finish_reason")]
    public string FinishReason { get; set; } = string.Empty;
    
    public int Index { get; set; }
}
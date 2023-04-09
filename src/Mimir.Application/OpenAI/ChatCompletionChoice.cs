
namespace Mimir.Application.OpenAI;

public class ChatCompletionChoice
{
    public GptMessage Message { get; set; }
    
    public string FinishReason { get; set; } = string.Empty;
    
    public int Index { get; set; }
}
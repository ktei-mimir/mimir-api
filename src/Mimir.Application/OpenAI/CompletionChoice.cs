
namespace Mimir.Application.OpenAI;

public class CompletionChoice
{
    public string Text { get; set; }
    
    public string FinishReason { get; set; } = string.Empty;
    
    public int Index { get; set; }
}
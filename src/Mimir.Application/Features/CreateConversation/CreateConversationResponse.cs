using Mimir.Application.ChatGpt;

namespace Mimir.Application.Features.CreateConversation;

public class CreateConversationResponse
{
    public string Id { get; set; }
    public string Title { get; set; }
    public ChatCompletionChoice[] Choices { get; set; }
    public int TotalTokens { get; set; }
}
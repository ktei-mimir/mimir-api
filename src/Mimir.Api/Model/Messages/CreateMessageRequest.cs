namespace Mimir.Api.Model.Messages;

public class CreateMessageRequest
{
    public string ConnectionId { get; set; } = string.Empty;
    public string ConversationId { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}

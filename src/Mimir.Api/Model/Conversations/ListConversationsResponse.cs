namespace Mimir.Api.Model.Conversations;

public class ListConversationsResponse
{
    public ConversationDto[] Items { get; set; } = Array.Empty<ConversationDto>();
}
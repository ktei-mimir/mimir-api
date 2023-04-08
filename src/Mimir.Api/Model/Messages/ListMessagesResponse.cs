namespace Mimir.Api.Model.Messages;

public class ListMessagesResponse
{
    public MessageDto[] Items { get; set; } = Array.Empty<MessageDto>();
}
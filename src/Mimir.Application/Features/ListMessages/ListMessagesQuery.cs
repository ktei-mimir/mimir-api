using MediatR;
using Mimir.Domain.Models;

namespace Mimir.Application.Features.ListMessages;

public class ListMessagesQuery : IRequest<List<Message>>
{
    public string Username { get; set; }
    public string ConversationId { get; set; }
    
    public ListMessagesQuery(string username, string conversationId)
    {
        Username = username;
        ConversationId = conversationId;
    }
}
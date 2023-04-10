using MediatR;
using Mimir.Application.OpenAI;
using Mimir.Domain.Models;

namespace Mimir.Application.Features.CreateMessage;

public class CreateMessageCommand : IRequest<Message>
{
    public string StreamId { get; set; }
    public string ConversationId { get; set; }
    public string Content { get; set; } = string.Empty;
    public string Role => Roles.User;
    
    public CreateMessageCommand(string streamId, string conversationId)
    {
        StreamId = streamId;
        ConversationId = conversationId;
    }
}
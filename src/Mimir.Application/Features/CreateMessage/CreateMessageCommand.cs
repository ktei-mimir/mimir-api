using MediatR;
using Mimir.Application.ChatGpt;
using Mimir.Domain.Models;

namespace Mimir.Application.Features.CreateMessage;

public class CreateMessageCommand : IRequest<Message>
{
    public string ConversationId { get; set; }
    public string Content { get; set; } = string.Empty;
    public string Role => Roles.User;
    
    public CreateMessageCommand(string conversationId)
    {
        ConversationId = conversationId;
    }
}
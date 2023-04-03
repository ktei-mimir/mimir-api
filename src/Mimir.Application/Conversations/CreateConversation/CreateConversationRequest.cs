using MediatR;

namespace Mimir.Application.Conversations.CreateConversation;

public class CreateConversationRequest : IRequest<CreateConversationResponse>
{
    public string Message { get; set; } = string.Empty;
}
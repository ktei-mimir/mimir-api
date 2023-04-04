using MediatR;

namespace Mimir.Application.Features.Conversations.CreateConversation;

public class CreateConversationCommand : IRequest<CreateConversationResponse>
{
    public string Message { get; set; } = string.Empty;
}
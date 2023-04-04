using MediatR;

namespace Mimir.Application.Features.CreateConversation;

public class CreateConversationCommand : IRequest<CreateConversationResponse>
{
    public string Message { get; set; } = string.Empty;
}
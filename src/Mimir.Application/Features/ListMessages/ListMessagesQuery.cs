using MediatR;
using Mimir.Domain.Models;

namespace Mimir.Application.Features.ListMessages;

public class ListMessagesQuery : IRequest<List<Message>>
{
    public string ConversationId { get; set; }
}
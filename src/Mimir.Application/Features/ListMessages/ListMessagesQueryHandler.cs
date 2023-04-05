using MediatR;
using Mimir.Application.Configurations;
using Mimir.Domain.Models;
using Mimir.Domain.Repositories;

namespace Mimir.Application.Features.ListMessages;

public class ListMessagesQueryHandler : IRequestHandler<ListMessagesQuery, List<Message>>
{
    private readonly IMessageRepository _messageRepository;

    public ListMessagesQueryHandler(IMessageRepository messageRepository)
    {
        _messageRepository = messageRepository;
    }

    public async Task<List<Message>> Handle(ListMessagesQuery request, CancellationToken cancellationToken)
    {
        var messages = await _messageRepository.ListByConversationId(request.ConversationId,
            Limits.MaxMessagesPerRequest,
            cancellationToken);
        return messages;
    }
}
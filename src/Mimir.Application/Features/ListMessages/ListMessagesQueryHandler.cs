using JetBrains.Annotations;
using MediatR;
using Mimir.Application.Configurations;
using Mimir.Application.OpenAI;
using Mimir.Domain.Models;
using Mimir.Domain.Repositories;

namespace Mimir.Application.Features.ListMessages;

[UsedImplicitly]
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
        var conversationMessages = messages.Where(x => x.Role is Roles.User or Roles.Assistant).ToList();
        return conversationMessages;
    }
}
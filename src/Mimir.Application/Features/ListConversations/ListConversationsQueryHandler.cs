using JetBrains.Annotations;
using MediatR;
using Mimir.Application.Configurations;
using Mimir.Domain.Models;
using Mimir.Domain.Repositories;

namespace Mimir.Application.Features.ListConversations;

[UsedImplicitly]
public class ListConversationsQueryHandler : IRequestHandler<ListConversationsQuery, List<Conversation>>
{
    private readonly IConversationRepository _conversationRepository;

    public ListConversationsQueryHandler(IConversationRepository conversationRepository)
    {
        _conversationRepository = conversationRepository;
    }

    public async Task<List<Conversation>> Handle(ListConversationsQuery request, CancellationToken cancellationToken)
    {
        var conversations = await _conversationRepository.ListByUsername(
            request.Username, Limits.MaxConversationsPerRequest, cancellationToken);
        return conversations;
    }
}
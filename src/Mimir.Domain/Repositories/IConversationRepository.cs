using Mimir.Domain.Models;

namespace Mimir.Domain.Repositories;

public interface IConversationRepository
{
    Task Create(Conversation conversation, Message firstMessage, CancellationToken cancellationToken = default);
    Task<List<Conversation>> List(int limit = 50, CancellationToken cancellationToken = default);
}

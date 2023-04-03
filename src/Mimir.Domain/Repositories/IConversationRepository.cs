using Mimir.Domain.Models;

namespace Mimir.Domain.Repositories;

public interface IConversationRepository
{
    Task Create(Conversation conversation, Message firstMessage, CancellationToken cancellationToken = default);
    Task<Conversation?> GetById(string id, CancellationToken cancellationToken = default);
}

using Mimir.Domain.Models;

namespace Mimir.Domain.Repositories;

public interface IConversationRepository
{
    Task Create(Conversation conversation, Message? firstMessage = null, CancellationToken cancellationToken = default);
    Task<List<Conversation>> ListAll(int limit = 50, CancellationToken cancellationToken = default);
}

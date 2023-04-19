using Mimir.Domain.Models;

namespace Mimir.Domain.Repositories;

public interface IConversationRepository
{
    Task Create(Conversation conversation, IEnumerable<Message>? messages = null,
        CancellationToken cancellationToken = default);

    Task<List<Conversation>> ListByUsername(string username, int limit = 50,
        CancellationToken cancellationToken = default);
}
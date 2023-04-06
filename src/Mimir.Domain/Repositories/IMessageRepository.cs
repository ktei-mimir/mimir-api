using Mimir.Domain.Models;

namespace Mimir.Domain.Repositories;

public interface IMessageRepository
{
    Task Create(IEnumerable<Message> messages, CancellationToken cancellationToken = default);

    Task<List<Message>> ListByConversationId(string conversationId, int limit = 10,
        CancellationToken cancellationToken = default);
}
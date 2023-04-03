using Mimir.Domain.Models;
using Mimir.Domain.Repositories;

namespace Mimir.Infrastructure.Repositories;

public class MessageRepository : IMessageRepository
{
    public Task Create(Message message)
    {
        throw new NotImplementedException();
    }

    public Task<List<Message>> GetByConversationId(string conversationId, int limit = 10)
    {
        throw new NotImplementedException();
    }
}
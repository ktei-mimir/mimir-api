using Mimir.Domain.Models;

namespace Mimir.Domain.Repositories;

public interface IMessageRepository
{
    Task Create(Message message);
    Task<List<Message>> GetByConversationId(string conversationId, int limit = 10);
}
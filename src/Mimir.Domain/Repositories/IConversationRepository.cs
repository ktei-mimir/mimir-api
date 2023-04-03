using Mimir.Domain.Models;

namespace Mimir.Domain.Repositories;

public interface IConversationRepository
{
    Task Create(Conversation conversation, Message message);
    Task<Conversation?> GetById(string id);
}

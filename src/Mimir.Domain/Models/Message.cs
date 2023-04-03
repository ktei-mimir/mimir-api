using Mimir.Domain.Helpers;

namespace Mimir.Domain.Models;

public class Message : Entity
{
    public string ConversationId { get; }
    public string Role { get; }
    public string Content { get; }
    public long CreatedAt { get; }

    public Message(string conversationId, string role, string content, long createdAt) : base(EntityTypes.Message)
    {
        ConversationId = conversationId;
        Role = role;
        Content = content;
        CreatedAt = createdAt;
    }

    public Message(string conversationId, string role, string content, DateTime createdAt)
        : this(conversationId, role, content, createdAt.ToUnixTimeStamp())
    {
    }
}

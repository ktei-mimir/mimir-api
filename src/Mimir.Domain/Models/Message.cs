using Mimir.Domain.Helpers;

namespace Mimir.Domain.Models;

public class Message : Entity
{
    public string ConversationId { get; private set; }
    public string Role { get; private set; }
    public string Content { get; private set; }
    public long CreatedAt { get; private set; }

    public Message() : base(EntityTypes.Message)
    {
        
    }

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

    public override string ToString() => $"Message: {ConversationId} - {Role}";
}

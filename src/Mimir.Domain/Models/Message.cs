using Mimir.Domain.Helpers;

namespace Mimir.Domain.Models;

public class Message : Entity
{
    private string _content;

    private string _role;

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
        : this(conversationId, role, content, createdAt.Ticks)
    {
    }

    public string ConversationId { get; private set; }

    public string Role
    {
        get => _role;
        private set
        {
            if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException($"{nameof(Role)} cannot be empty");

            _role = value;
        }
    }

    public string Content
    {
        get => _content;
        private set
        {
            if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException($"{nameof(Content)} cannot be empty");

            _content = value;
        }
    }

    public long CreatedAt { get; private set; }

    public override string ToString()
    {
        return $"Message: {Role} - {Content.TakeMax(10)}";
    }
}
using Mimir.Domain.Helpers;

namespace Mimir.Domain.Models;

public class Message : Entity
{
    public const int MaxContentLength = 1000;
    public string ConversationId { get; private set; }

    private string _role;

    public string Role
    {
        get => _role;
        private set
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException($"{nameof(Role)} cannot be empty"); 
            }

            _role = value;
        }
    }

    private string _content;
    public string Content
    {
        get => _content;
        private set
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException($"{nameof(Content)} cannot be empty");
            }

            if (value.Length > MaxContentLength)
                throw new ArgumentOutOfRangeException(
                    $"The length of {nameof(Content)} cannot exceed {MaxContentLength} characters");

            _content = value;
        }
    }
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

    public override string ToString() => $"Message: {Role} - {Content.TakeMax(10)}";
}

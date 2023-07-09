using Mimir.Domain.Helpers;

namespace Mimir.Domain.Models;

public class Message : Entity
{
    private string _role;

    public Message() : base(EntityTypes.Message)
    {
    }

    public Message(string conversationId, string role, string content, string? streamId, long createdAt) : base(
        EntityTypes.Message)
    {
        ConversationId = conversationId;
        Role = role;
        Content = content;
        StreamId = streamId;
        CreatedAt = createdAt;
    }

    public Message(string conversationId, string role, string content, string? streamId, DateTime createdAt)
        : this(conversationId, role, content, streamId, createdAt.Ticks)
    {
    }


    public string ConversationId { get; private set; }

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

    public string Content { get; private set; }

    public string? StreamId { get; private set; }

    public long CreatedAt { get; private set; }

    public static Message UserMessage(string conversationId, string content, DateTime createdAt)
    {
        return new Message(conversationId, Roles.User, content, null, createdAt);
    }

    public static Message AssistantMessage(string conversationId, string content, string streamId, DateTime createdAt)
    {
        return new Message(conversationId, Roles.Assistant, content, streamId, createdAt);
    }

    public void UpdateContent(string content)
    {
        Content = content;
    }

    public override string ToString()
    {
        return $"Message: {Role} - {Content.TakeMax(10)}";
    }
}

public static class Roles
{
    public const string User = "user";
    public const string Assistant = "assistant";
    public const string System = "system";
}
using Mimir.Domain.Helpers;

namespace Mimir.Domain.Models;

public class Conversation : Entity
{
    public string Id { get; private set; }
    public string Username { get; private set; }
    public string Title { get; private set; }
    public long CreatedAt { get; private set; }

    public Conversation() : base(EntityTypes.Conversation)
    {

    }

    public Conversation(string id, string username, string title, DateTime createdAt) : this(id, username, title,
        createdAt.ToUnixTimeStamp())
    {
    }

    public Conversation(string id, string username, string title, long createdAt) : base(EntityTypes.Conversation)
    {
        Id = id;
        Username = username;
        Title = title;
        CreatedAt = createdAt;
    }

    public override string ToString()
    {
        return $"Conversation: {Title}";
    }
}

using Mimir.Domain.Helpers;

namespace Mimir.Domain.Models;

public class Conversation : Entity
{
    public string Id { get; }
    public string Title { get; }
    public long CreatedAt { get; }

    public Conversation(string id, string title, DateTime createdAt) : this(id, title, createdAt.ToUnixTimeStamp())
    {
    }

    public Conversation(string id, string title, long createdAt) : base(EntityTypes.Conversation)
    {
        Id = id;
        Title = title;
        CreatedAt = createdAt;
    }
}

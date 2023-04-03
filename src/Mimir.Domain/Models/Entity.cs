namespace Mimir.Domain.Models;

public abstract class Entity
{
    public string Type { get; }

    protected Entity(string type)
    {
        Type = type;
    }
}
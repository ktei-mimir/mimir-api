namespace Mimir.Api.Model.Messages;

public class MessageDto
{
    public long CreatedAt { get; set; }
    public string Role { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}
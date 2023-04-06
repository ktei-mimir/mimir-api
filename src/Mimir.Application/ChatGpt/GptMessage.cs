namespace Mimir.Application.ChatGpt;

public class GptMessage
{
    public string Role { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;

    public override string ToString() => Content[..Math.Min(Content.Length, 10)];
}
using Mimir.Application.ChatGpt;

namespace Mimir.Application.Conversations.CreateConversation;

public class CreateConversationResponse
{
    public string Id { get; }
    public IReadOnlyCollection<Choice> Choices { get; }
    public Usage Usage { get; }

    public CreateConversationResponse(string id, IReadOnlyCollection<Choice> choices, Usage usage)
    {
        Id = id;
        Choices = choices;
        Usage = usage;
    }
}
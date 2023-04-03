using MediatR;
using Mimir.Application.ChatGpt;

namespace Mimir.Application.Conversations.CreateConversation;

public class CreateConversationRequestHandler : IRequestHandler<CreateConversationRequest, CreateConversationResponse>
{
    private readonly IChatGptApi _chatGptApi;

    public CreateConversationRequestHandler(IChatGptApi chatGptApi)
    {
        _chatGptApi = chatGptApi;
    }

    public async Task<CreateConversationResponse> Handle(CreateConversationRequest request,
        CancellationToken cancellationToken)
    {
        var newConversationId = Guid.NewGuid().ToString();
        var chatGptResponse = await _chatGptApi.CreateChatCompletion(new CreateChatCompletionRequest
        {
            Messages = new List<Message> { new() { Role = Roles.User, Content = request.Message } }
        });
        var response =
            new CreateConversationResponse(newConversationId, chatGptResponse.Choices, chatGptResponse.Usage);

        return response;
    }
}
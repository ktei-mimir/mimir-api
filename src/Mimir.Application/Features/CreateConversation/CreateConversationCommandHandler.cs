using JetBrains.Annotations;
using MediatR;
using Mimir.Application.Interfaces;
using Mimir.Application.OpenAI;
using Mimir.Application.Security;
using Mimir.Domain.Models;
using Mimir.Domain.Repositories;

namespace Mimir.Application.Features.CreateConversation;

[UsedImplicitly]
public class CreateConversationCommandHandler : IRequestHandler<CreateConversationCommand, CreateConversationResponse>
{
    private readonly IChatGptApi _chatGptApi;
    private readonly IUserIdentityProvider _userIdentityProvider;
    private readonly IConversationRepository _conversationRepository;
    private readonly IDateTime _dateTime;

    public CreateConversationCommandHandler(IChatGptApi chatGptApi, IUserIdentityProvider userIdentityProvider,
        IConversationRepository conversationRepository, IDateTime dateTime)
    {
        _chatGptApi = chatGptApi;
        _userIdentityProvider = userIdentityProvider;
        _conversationRepository = conversationRepository;
        _dateTime = dateTime;
    }

    public async Task<CreateConversationResponse> Handle(CreateConversationCommand command,
        CancellationToken cancellationToken)
    {
        var username = _userIdentityProvider.GetUsername();
        var newConversationId = Guid.NewGuid().ToString();
        var completion = await _chatGptApi.CreateCompletion(new CreateCompletionRequest
        {
            Prompt = CommandBuilder.Summarize(command.Message),
            MaxTokens = 20
        }, cancellationToken);
        var conversationTitle = completion.Choices.First().Text;
        await _conversationRepository.Create(
            new Conversation(newConversationId, username, conversationTitle, _dateTime.UtcNow()),
            cancellationToken);

        var response = new CreateConversationResponse
        {
            Id = newConversationId,
            Title = conversationTitle,
        };

        return response;
    }
}

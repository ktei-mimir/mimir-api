using JetBrains.Annotations;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using Mimir.Application.Configurations;
using Mimir.Application.Interfaces;
using Mimir.Application.OpenAI;
using Mimir.Application.RealTime;
using Mimir.Application.Security;
using Mimir.Domain.Exceptions;
using Mimir.Domain.Models;
using Mimir.Domain.Repositories;

namespace Mimir.Application.Features.CreateMessage;

[UsedImplicitly]
public class CreateMessageCommandHandler : IRequestHandler<CreateMessageCommand, Message>
{
    private readonly IChatGptApi _chatGptApi;
    private readonly IDateTime _dateTime;
    private readonly IHubContext<ConversationHub, IConversationClient> _hubContext;
    private readonly IMessageRepository _messageRepository;
    private readonly IUserIdentityProvider _userIdentityProvider;

    public CreateMessageCommandHandler(IMessageRepository messageRepository, IChatGptApi chatGptApi, IDateTime dateTime,
        IHubContext<ConversationHub, IConversationClient> hubContext, IUserIdentityProvider userIdentityProvider)
    {
        _messageRepository = messageRepository;
        _chatGptApi = chatGptApi;
        _dateTime = dateTime;
        _hubContext = hubContext;
        _userIdentityProvider = userIdentityProvider;
    }

    public async Task<Message> Handle(CreateMessageCommand command, CancellationToken cancellationToken)
    {
        var historyMessages = await _messageRepository.ListByConversationId(command.ConversationId,
            Limits.MaxMessagesPerRequest, cancellationToken);
        var userMessage = new Message(command.ConversationId, command.Role, command.Content, _dateTime.UtcNow());

        // pass the history messages + user message to the GPT
        var username = _userIdentityProvider.GetUsername();
        var hubUser = _hubContext.Clients.User(username);
        var chatCompletion = await _chatGptApi.CreateChatCompletion(new CreateChatCompletionRequest
        {
            Messages = historyMessages.Concat(new[] { userMessage }).Select(x => new GptMessage
            {
                Role = x.Role,
                Content = x.Content
            }).ToList()
        }, messageContent => hubUser?.StreamMessage(new StreamMessageRequest
        {
            StreamId = command.StreamId,
            ConversationId = command.ConversationId,
            Content = messageContent
        }), cancellationToken);

        // there should be at least one choice
        if (!chatCompletion.Choices.Any())
            throw new NoChoiceProvidedException();

        // TODO: what do we do with multiple choices?
        // pick the first choice
        var firstChoice = chatCompletion.Choices.First();
        var assistantMessage = new Message(command.ConversationId, firstChoice.Message.Role,
            firstChoice.Message.Content, _dateTime.UtcNow());

        // save both user message and assistant message
        await _messageRepository.Create(new[] { userMessage, assistantMessage }, cancellationToken);
        return assistantMessage;
    }
}
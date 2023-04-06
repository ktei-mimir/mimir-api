using JetBrains.Annotations;
using MediatR;
using Mimir.Application.ChatGpt;
using Mimir.Application.Configurations;
using Mimir.Domain.Exceptions;
using Mimir.Domain.Models;
using Mimir.Domain.Repositories;

namespace Mimir.Application.Features.CreateMessage;

[UsedImplicitly]
public class CreateMessageCommandHandler : IRequestHandler<CreateMessageCommand, Message>
{
    private readonly IMessageRepository _messageRepository;
    private readonly IChatGptApi _chatGptApi;

    public CreateMessageCommandHandler(IMessageRepository messageRepository, IChatGptApi chatGptApi)
    {
        _messageRepository = messageRepository;
        _chatGptApi = chatGptApi;
    }

    public async Task<Message> Handle(CreateMessageCommand command, CancellationToken cancellationToken)
    {
        var historyMessages = await _messageRepository.ListByConversationId(command.ConversationId, 
            Limits.MaxMessagesPerRequest, cancellationToken);
        var userMessage = new Message(command.ConversationId, command.Role, command.Content, DateTime.UtcNow);
        
        // pass the history messages + user message to the GPT
        var chatCompletion = await _chatGptApi.CreateChatCompletion(new CreateChatCompletionRequest
        {
            Messages = historyMessages.Concat(new[] { userMessage }).Select(x => new GptMessage
            {
                Role = x.Role,
                Content = x.Content
            }).ToList()
        }, cancellationToken);
        
        // there should be at least one choice
        if (!chatCompletion.Choices.Any())
            throw new NoChoiceProvidedException();

        // TODO: what do we do with multiple choices?
        // pick the first choice
        var firstChoice = chatCompletion.Choices.First();
        var assistantMessage = new Message(command.ConversationId, firstChoice.Message.Role,
            firstChoice.Message.Content, chatCompletion.Created);
        
        // save both user message and assistant message
        await _messageRepository.Create(new[] { userMessage, assistantMessage }, cancellationToken);
        return assistantMessage;
    }
}

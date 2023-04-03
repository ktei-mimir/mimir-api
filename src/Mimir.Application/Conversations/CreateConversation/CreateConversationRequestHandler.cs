using MediatR;
using Mimir.Application.ChatGpt;
using Mimir.Domain.Models;
using Mimir.Domain.Repositories;
using ChatGptMessage = Mimir.Application.ChatGpt.Message;
using Message = Mimir.Domain.Models.Message;

namespace Mimir.Application.Conversations.CreateConversation;

public class CreateConversationRequestHandler : IRequestHandler<CreateConversationRequest, CreateConversationResponse>
{
    private readonly IChatGptApi _chatGptApi;
    private readonly IConversationRepository _conversationRepository;
    private readonly IMessageRepository _messageRepository;

    public CreateConversationRequestHandler(IChatGptApi chatGptApi, IConversationRepository conversationRepository,
        IMessageRepository messageRepository)
    {
        _chatGptApi = chatGptApi;
        _conversationRepository = conversationRepository;
        _messageRepository = messageRepository;
    }

    public async Task<CreateConversationResponse> Handle(CreateConversationRequest request,
        CancellationToken cancellationToken)
    {
        var newConversationId = Guid.NewGuid().ToString();
        var completion = await _chatGptApi.CreateCompletion(new CreateCompletionRequest
        {
            Prompt = CommandBuilder.Summarize(request.Message),
            MaxTokens = 20
        });
        var conversationTitle = completion.Choices.First().Text;
        await _conversationRepository.Create(new Conversation(newConversationId, conversationTitle, DateTime.UtcNow),
            new Message(newConversationId, Roles.User, request.Message, DateTime.UtcNow),
            cancellationToken);
        
        var chatCompletion = await _chatGptApi.CreateChatCompletion(new CreateChatCompletionRequest
        {
            Messages = new List<ChatGptMessage> { new() { Role = Roles.User, Content = request.Message } }
        });
        await _messageRepository.Create(new Message(newConversationId, Roles.Assistant, 
            chatCompletion.Choices.First().Message.Content, chatCompletion.Created), cancellationToken);
        
        var response =
            new CreateConversationResponse(newConversationId, chatCompletion.Choices,
                chatCompletion.Usage.Add(chatCompletion.Usage));

        return response;
    }
}

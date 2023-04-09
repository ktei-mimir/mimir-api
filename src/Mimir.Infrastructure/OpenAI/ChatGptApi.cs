using Microsoft.Extensions.Options;
using Mimir.Application.OpenAI;
using Mimir.Infrastructure.Configurations;
using OpenAI_API;
using OpenAI_API.Chat;
using OpenAI_API.Completions;
using OpenAI_API.Models;
using Usage = Mimir.Application.OpenAI.Usage;

namespace Mimir.Infrastructure.OpenAI;

public class ChatGptApi : IChatGptApi
{
    private readonly OpenAIAPI _api;

    public ChatGptApi(IOptions<OpenAIOptions> optionsAccessor)
    {
        _api = new OpenAIAPI(optionsAccessor.Value.ApiKey);
    }

    public async Task<ChatCompletion> CreateChatCompletion(CreateChatCompletionRequest request, CancellationToken cancellationToken = default)
    {
        var chat = _api.Chat.CreateConversation();
        foreach (var message in request.Messages)
        {
            switch (message.Role)
            {
                case Roles.Assistant:
                    chat.AppendMessage(ChatMessageRole.Assistant, message.Content);
                    break;
                case Roles.User:
                    chat.AppendUserInput(message.Content);
                    break;
                default:
                    throw new NotSupportedException($"Role {message.Role} is not supported");
            }
        }

        await chat.GetResponseFromChatbotAsync();
        var result = chat.MostResentAPIResult;
        return new ChatCompletion
        {
            Id = result.Id,
            Object = result.Object,
            Model = result.Model,
            Choices = result.Choices.Select(x => new ChatCompletionChoice
            {
                Index = x.Index,
                Message = new GptMessage
                {
                    Role = x.Message.Role,
                    Content = x.Message.Content
                },
                FinishReason = x.FinishReason
            }).ToList()
        };
    }

    public async Task<Completion> CreateCompletion(CreateCompletionRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _api.Completions.CreateCompletionAsync(new CompletionRequest(request.Prompt, model: Model.DavinciText));
        return new Completion
        {
            Choices = result.Completions.Select(x => new CompletionChoice
            {
                Index = x.Index,
                Text = x.Text,
                FinishReason = x.FinishReason
            }).ToList(),
            Usage = new Usage
            {
                PromptTokens = result.Usage.PromptTokens,
                CompletionTokens = result.Usage.CompletionTokens,
                TotalTokens = result.Usage.TotalTokens
            }
        };
    }
}

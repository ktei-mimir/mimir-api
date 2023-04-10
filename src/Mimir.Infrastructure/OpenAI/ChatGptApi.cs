using System.Text;
using Mimir.Application.OpenAI;
using Mimir.Domain.Exceptions;
using OpenAI.GPT3.Interfaces;
using OpenAI.GPT3.ObjectModels.RequestModels;
using ChatMessage = OpenAI.GPT3.ObjectModels.RequestModels.ChatMessage;
using Usage = Mimir.Application.OpenAI.Usage;

namespace Mimir.Infrastructure.OpenAI;

public class ChatGptApi : IChatGptApi
{
    private readonly IOpenAIService _openAIService;

    public ChatGptApi(IOpenAIService openAIService)
    {
        _openAIService = openAIService;
    }

    public async Task<ChatCompletion> CreateChatCompletion(CreateChatCompletionRequest request,
        Action<string>? resultHandler = null,
        CancellationToken cancellationToken = default)
    {
        var completionResult = _openAIService.ChatCompletion.CreateCompletionAsStream(new ChatCompletionCreateRequest
        {
            Model = OpenAIModels.Gpt3Turbo,
            Messages = request.Messages.Select(m => m.Role switch
            {
                Roles.User => ChatMessage.FromUser(m.Content),
                Roles.Assistant => ChatMessage.FromAssistant(m.Content),
                _ => throw new NotSupportedException($"Role {m.Role} is not supported")
            }).ToList()
        }, cancellationToken: cancellationToken);

        var messageContentBuilder = new StringBuilder();
        await foreach (var completion in completionResult.WithCancellation(cancellationToken))
            if (completion.Successful)
            {
                if (completion.Choices.FirstOrDefault()?.Delta is not { } delta) continue;
                messageContentBuilder.Append(delta.Content);
                resultHandler?.Invoke(messageContentBuilder.ToString());
            }
            else
            {
                if (completion.Error == null)
                    throw new ChatCompletionException("Chat completion failed due to unknown error");

                throw new ChatCompletionException(
                    $"Chat completion failed: {completion.Error.Code}- {completion.Error.Message}");
            }

        return new ChatCompletion
        {
            Choices = new List<ChatCompletionChoice>
            {
                new()
                {
                    Message = new GptMessage
                    {
                        Role = Roles.Assistant,
                        Content = messageContentBuilder.ToString()
                    },
                    FinishReason = "stop"
                }
            }
        };
    }

    public async Task<Completion> CreateCompletion(CreateCompletionRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _openAIService.Completions.CreateCompletion(new CompletionCreateRequest
        {
            Prompt = request.Prompt,
            Model = OpenAIModels.Davinci,
        }, cancellationToken: cancellationToken);
        return new Completion
        {
            Choices = result.Choices.Select(x => new CompletionChoice
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
using System.Text;
using Mimir.Application.OpenAI;
using Mimir.Domain.Exceptions;
using OpenAI.GPT3.Interfaces;
using OpenAI.GPT3.ObjectModels.RequestModels;
using Polly;

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
        async Task<ChatCompletion> ExecuteChatCompletion(CreateChatCompletionRequest createChatCompletionRequest,
            Action<string>? action,
            CancellationToken ct)
        {
            var messages = createChatCompletionRequest.Messages.Select(m => m.Role switch
            {
                Roles.User => ChatMessage.FromUser(m.Content),
                Roles.Assistant => ChatMessage.FromAssistant(m.Content),
                _ => throw new NotSupportedException($"Role {m.Role} is not supported")
            }).ToList();
            AppendSystemMessage(messages,
                "If your answer contains any code snippet, please use the code block syntax for code snippets.");
            var completionResult = _openAIService.ChatCompletion.CreateCompletionAsStream(
                new ChatCompletionCreateRequest
                {
                    Model = OpenAIModels.Gpt3Turbo,
                    Messages = messages
                }, cancellationToken: ct);

            var messageContentBuilder = new StringBuilder();
            await foreach (var completion in completionResult.WithCancellation(ct))
                if (completion.Successful)
                {
                    if (completion.Choices.FirstOrDefault()?.Delta is not { } delta) continue;
                    messageContentBuilder.Append(delta.Content);
                    action?.Invoke(messageContentBuilder.ToString());
                }
                else
                {
                    if (completion.Error == null)
                        throw new OpenAIAPIException("Chat completion failed due to unknown error");

                    throw new OpenAIAPIException(
                        $"Chat completion failed: {completion.Error.Code} - {completion.Error.Message}");
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

        return await Policy
            .Handle<OpenAIAPIException>()
            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)))
            .ExecuteAsync(() => ExecuteChatCompletion(request, resultHandler, cancellationToken));
    }

    public async Task<Completion> CreateCompletion(CreateCompletionRequest request,
        CancellationToken cancellationToken = default)
    {
        async Task<Completion> ExecuteCompletion(CreateCompletionRequest createCompletionRequest, CancellationToken ct)
        {
            var result = await _openAIService.Completions.CreateCompletion(new CompletionCreateRequest
            {
                Prompt = createCompletionRequest.Prompt,
                Model = OpenAIModels.Davinci
            }, cancellationToken: ct);
            if (result.Error != null)
                throw new OpenAIAPIException(
                    $"Completion failed: {result.Error.Code} - {result.Error.Message}");
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

        return await Policy
            .Handle<OpenAIAPIException>()
            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)))
            .ExecuteAsync(() => ExecuteCompletion(request, cancellationToken));
    }

    /// <summary>
    ///     Append System ChatMessage before the last item in a message list
    /// </summary>
    private static void AppendSystemMessage(List<ChatMessage> messages, string content)
    {
        messages.Insert(messages.Count - 1, ChatMessage.FromSystem(content));
    }
}
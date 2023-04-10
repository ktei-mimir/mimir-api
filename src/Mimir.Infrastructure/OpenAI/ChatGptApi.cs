using System.Text;
using Microsoft.Extensions.Options;
using Mimir.Application.OpenAI;
using Mimir.Infrastructure.Configurations;
using OpenAI_API;
using OpenAI_API.Chat;
using OpenAI_API.Completions;
using OpenAI_API.Models;
using OpenAI.GPT3.Interfaces;
using OpenAI.GPT3.ObjectModels.RequestModels;
using OpenAI.GPT3.ObjectModels.SharedModels;
using ChatMessage = OpenAI.GPT3.ObjectModels.RequestModels.ChatMessage;
using Usage = Mimir.Application.OpenAI.Usage;

namespace Mimir.Infrastructure.OpenAI;

public class ChatGptApi : IChatGptApi
{
    private readonly OpenAIAPI _api;
    private readonly IOpenAIService _openAIService;

    public ChatGptApi(IOptions<OpenAIOptions> optionsAccessor, IOpenAIService openAIService)
    {
        _openAIService = openAIService;
        _api = new OpenAIAPI(optionsAccessor.Value.ApiKey);
    }

    public async Task<ChatCompletion> CreateChatCompletion(CreateChatCompletionRequest request, 
        Action<string>? resultHandler = null,
        CancellationToken cancellationToken = default)
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

        // var completionResult = _openAIService.ChatCompletion.CreateCompletionAsStream(new ChatCompletionCreateRequest
        // {
        //     Model = "gpt-3.5-turbo",
        //     
        //     Messages = request.Messages.Select(m => m.Role switch
        //     {
        //         Roles.User => ChatMessage.FromUser(m.Content),
        //         Roles.Assistant => ChatMessage.FromAssistant(m.Content),
        //         _ => throw new NotSupportedException($"Role {m.Role} is not supported")
        //     }).ToList()
        // }, cancellationToken: cancellationToken);

        // await foreach (var completion in completionResult.WithCancellation(cancellationToken))
        // {
        //     if (completion.Successful)
        //     {
        //         Console.Write(completion.Choices.FirstOrDefault()?.Delta.Content);
        //     }
        //     else
        //     {
        //         if (completion.Error == null)
        //         {
        //             throw new Exception("Unknown Error");
        //         }
        //
        //         Console.WriteLine($"{completion.Error.Code}: {completion.Error.Message}");
        //     }
        // }
        
        // Console.WriteLine("Complete");

        // await chat.GetResponseFromChatbotAsync();

        var messageContentBuilder = new StringBuilder();
        await chat.StreamResponseFromChatbotAsync(delta =>
        {
            messageContentBuilder.Append(delta);
            resultHandler?.Invoke(messageContentBuilder.ToString());
        });
        var result = chat.MostResentAPIResult;
        return new ChatCompletion
        {
            Id = result.Id,
            Object = result.Object,
            Model = result.Model,
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

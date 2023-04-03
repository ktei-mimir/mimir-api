using Refit;

namespace Mimir.Application.ChatGpt;

public interface IChatGptApi
{
    [Post("/v1/chat/completions")]
    Task<ChatCompletion> CreateChatCompletion([Body] CreateChatCompletionRequest request);
    
    [Post("/v1/completions")]
    Task<Completion> CreateCompletion([Body] CreateCompletionRequest request);
}

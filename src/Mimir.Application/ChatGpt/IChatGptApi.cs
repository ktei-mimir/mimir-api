using Refit;

namespace Mimir.Application.ChatGpt;

public interface IChatGptApi
{
    [Post("/v1/chat/completions")]
    Task<ChatCompletion> CreateChatCompletion([Body] CreateChatCompletionRequest request,
        CancellationToken cancellationToken = default);
    
    [Post("/v1/completions")]
    Task<Completion> CreateCompletion([Body] CreateCompletionRequest request,
        CancellationToken cancellationToken = default);
}


namespace Mimir.Application.OpenAI;

public interface IChatGptApi
{
    Task<ChatCompletion> CreateChatCompletion(CreateChatCompletionRequest request,
        Action<string>? resultHandler = null,
        CancellationToken cancellationToken = default);
    
    Task<Completion> CreateCompletion(CreateCompletionRequest request,
        CancellationToken cancellationToken = default);
}

namespace Mimir.Application.RealTime;

public interface IConversationClient
{
    /// <summary>
    /// Stream a message to client. This is used to provide a better user experience
    /// instead of letting the user wait for the response.
    /// </summary>
    Task StreamMessage(StreamMessageRequest request);
}

public class StreamMessageRequest
{
    /// <summary>
    /// When we receive a message with this property from the client,
    /// we need to stream the response back to it.
    /// </summary>
    public string StreamId { get; set; }
    public string ConversationId { get; set; }
    public string Content { get; set; }
}

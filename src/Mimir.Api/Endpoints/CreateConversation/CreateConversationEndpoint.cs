using FastEndpoints;
using MediatR;
using Mimir.Application.Conversations.CreateConversation;

namespace Mimir.Api.Endpoints.CreateConversation;

public class CreateConversationEndpoint : Endpoint<CreateConversationRequest, CreateConversationResponse>
{
    private readonly ISender _sender;
    
    public CreateConversationEndpoint(ISender sender)
    {
        _sender = sender;
    }

    public override void Configure()
    {
        Post("/v1/conversations");
        Policies("ChatGptUserOnly");
    }

    public override async Task HandleAsync(CreateConversationRequest request, CancellationToken cancellationToken)
    {
        var response = await _sender.Send(request, cancellationToken);
        await SendAsync(response, cancellation: cancellationToken);
    }
}
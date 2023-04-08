using FastEndpoints;
using JetBrains.Annotations;
using MediatR;
using Mimir.Api.Model.Messages;
using Mimir.Api.Security;
using Mimir.Application.Features.ListMessages;
using IMapper = AutoMapper.IMapper;

namespace Mimir.Api.Endpoints.ListMessages;

[PublicAPI]
public class ListMessagesEndpoint : EndpointWithoutRequest<ListMessagesResponse>
{
    private readonly ISender _sender;
    private readonly IMapper _mapper;

    public ListMessagesEndpoint(ISender sender, IMapper mapper)
    {
        _sender = sender;
        _mapper = mapper;
    }

    public override void Configure()
    {
        Get("/v1/conversations/{conversationId}/messages");
        this.RequireChatGptUser();
    }
    
    public override async Task<ListMessagesResponse> ExecuteAsync(CancellationToken ct)
    {
        var conversationId = Route<string>("conversationId")!;
        var query = new ListMessagesQuery { ConversationId = conversationId };
        var messages = await _sender.Send(query, ct);
        var items = _mapper.Map<MessageDto[]>(messages);
        return new ListMessagesResponse { Items = items };
    }
}

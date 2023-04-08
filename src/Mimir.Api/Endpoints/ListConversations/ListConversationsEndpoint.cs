using FastEndpoints;
using JetBrains.Annotations;
using MediatR;
using Mimir.Api.Model.Conversations;
using Mimir.Api.Security;
using Mimir.Application.Features.ListConversations;
using IMapper = AutoMapper.IMapper;

namespace Mimir.Api.Endpoints.ListConversations;

[PublicAPI]
public class ListConversationsEndpoint : EndpointWithoutRequest<ListConversationsResponse>
{
    private readonly ISender _sender;
    private readonly IMapper _mapper;

    public ListConversationsEndpoint(ISender sender, IMapper mapper)
    {
        _sender = sender;
        _mapper = mapper;
    }

    public override void Configure()
    {
        Get("/v1/conversations");
        this.RequireChatGptUser();
    }

    public override async Task<ListConversationsResponse> ExecuteAsync(CancellationToken ct)
    {
        var query = new ListConversationsQuery();
        var conversations = await _sender.Send(query, ct);
        var items = _mapper.Map<ConversationDto[]>(conversations);
        return new ListConversationsResponse { Items = items };
    }
}
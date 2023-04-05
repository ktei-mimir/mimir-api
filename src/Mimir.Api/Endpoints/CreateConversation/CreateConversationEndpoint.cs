using FastEndpoints;
using MediatR;
using Mimir.Api.Model.Conversations;
using Mimir.Api.Security;
using Mimir.Application.Features.CreateConversation;
using IMapper = AutoMapper.IMapper;

namespace Mimir.Api.Endpoints.CreateConversation;

public class CreateConversationEndpoint : Endpoint<CreateConversationRequest, CreateConversationResponse>
{
    private readonly ISender _sender;
    private readonly IMapper _mapper;

    public CreateConversationEndpoint(ISender sender, IMapper mapper)
    {
        _sender = sender;
        _mapper = mapper;
    }

    public override void Configure()
    {
        Post("/v1/conversations");
        this.RequireChatGptUser();
    }

    public override async Task<CreateConversationResponse> ExecuteAsync(CreateConversationRequest request,
        CancellationToken cancellationToken)
    {
        var command = _mapper.Map<CreateConversationCommand>(request);
        var response = await _sender.Send(command, cancellationToken);
        return response;
    }
}
using FastEndpoints;
using JetBrains.Annotations;
using MediatR;
using Mimir.Api.Model.Messages;
using Mimir.Api.Security;
using IMapper = AutoMapper.IMapper;

namespace Mimir.Api.Endpoints.CreateMessage;

[PublicAPI]
public class CreateMessageEndpoint : Endpoint<CreateMessageRequest, MessageDto>
{
    private readonly ISender _sender;
    private readonly IMapper _mapper;

    public CreateMessageEndpoint(ISender sender, IMapper mapper)
    {
        _sender = sender;
        _mapper = mapper;
    }

    public override void Configure()
    {
        Post("/v1/conversations/{conversationId}/messages");
        this.RequireChatGptUser();
    }

    public override async Task<MessageDto> ExecuteAsync(CreateMessageRequest request, CancellationToken ct)
    {
        var message = await _sender.Send(request, ct);
        var response = _mapper.Map<MessageDto>(message);
        return response;
    }
}
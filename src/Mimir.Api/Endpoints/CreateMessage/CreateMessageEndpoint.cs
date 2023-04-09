using FastEndpoints;
using JetBrains.Annotations;
using MediatR;
using Mimir.Api.Model.Messages;
using Mimir.Api.Security;
using Mimir.Application.Features.CreateMessage;
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
        var conversationId = Route<string>("conversationId")!;
        var command = new CreateMessageCommand(request.ConnectionId, conversationId)
        {
            Content = request.Content
        };
        var message = await _sender.Send(command, ct);
        var response = _mapper.Map<MessageDto>(message);
        return response;
    }
}

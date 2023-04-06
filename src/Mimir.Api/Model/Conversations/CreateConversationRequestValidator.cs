using FastEndpoints;
using JetBrains.Annotations;
using Mimir.Domain.Models;

namespace Mimir.Api.Model.Conversations;

[UsedImplicitly]
public class CreateConversationRequestValidator : Validator<CreateConversationRequest>
{
    public CreateConversationRequestValidator()
    {
        RuleFor(x => x.Message)
            .NotEmpty()
            .MaximumLength(Message.MaxContentLength);
    }
}
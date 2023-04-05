using FastEndpoints;
using Mimir.Domain.Models;

namespace Mimir.Api.Model.Conversations;

public class CreateConversationRequestValidator : Validator<CreateConversationRequest>
{
    public CreateConversationRequestValidator()
    {
        RuleFor(x => x.Message)
            .NotEmpty()
            .MaximumLength(Message.MaxContentLength);
    }
}
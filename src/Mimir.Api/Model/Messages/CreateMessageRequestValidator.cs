using JetBrains.Annotations;
using Mimir.Domain.Models;

namespace Mimir.Api.Model.Messages;

[UsedImplicitly]
public class CreateMessageRequestValidator : AbstractValidator<CreateMessageRequest>
{
    public CreateMessageRequestValidator()
    {
        RuleFor(x => x.ConversationId)
            .NotEmpty();

        RuleFor(x => x.Content)
            .NotEmpty()
            .MaximumLength(Message.MaxContentLength);
    }
}

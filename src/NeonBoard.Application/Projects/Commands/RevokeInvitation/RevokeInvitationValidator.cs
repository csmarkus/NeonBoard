using FluentValidation;
using NeonBoard.Application.Common;

namespace NeonBoard.Application.Projects.Commands.RevokeInvitation;

public class RevokeInvitationValidator : AbstractValidator<RevokeInvitationCommand>
{
    public RevokeInvitationValidator()
    {
        RuleFor(x => x.ProjectId).NotEmpty().WithMessage(ValidationMessages.ProjectIdRequired);
        RuleFor(x => x.InvitationId).NotEmpty();
    }
}

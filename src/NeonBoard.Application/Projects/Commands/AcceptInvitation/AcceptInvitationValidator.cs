using FluentValidation;
using NeonBoard.Application.Common;

namespace NeonBoard.Application.Projects.Commands.AcceptInvitation;

public class AcceptInvitationValidator : AbstractValidator<AcceptInvitationCommand>
{
    public AcceptInvitationValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty()
            .WithMessage(ValidationMessages.InvitationTokenRequired);

        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage(ValidationMessages.MemberUserIdRequired);
    }
}

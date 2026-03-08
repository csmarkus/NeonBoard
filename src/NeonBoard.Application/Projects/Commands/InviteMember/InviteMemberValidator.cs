using FluentValidation;
using NeonBoard.Application.Common;
using NeonBoard.Domain.Projects;

namespace NeonBoard.Application.Projects.Commands.InviteMember;

public class InviteMemberValidator : AbstractValidator<InviteMemberCommand>
{
    public InviteMemberValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotEmpty()
            .WithMessage(ValidationMessages.ProjectIdRequired);

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage(ValidationMessages.InvitationEmailRequired)
            .EmailAddress()
            .WithMessage(ValidationMessages.InvitationEmailInvalid);

        RuleFor(x => x.Role)
            .Must(r => r == ProjectRole.Editor || r == ProjectRole.Viewer)
            .WithMessage(ValidationMessages.InvitationRoleInvalid);

        RuleFor(x => x.InvitedByUserId)
            .NotEmpty()
            .WithMessage(ValidationMessages.OwnerIdRequired);
    }
}

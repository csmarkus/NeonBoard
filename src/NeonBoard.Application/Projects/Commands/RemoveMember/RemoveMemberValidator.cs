using FluentValidation;
using NeonBoard.Application.Common;

namespace NeonBoard.Application.Projects.Commands.RemoveMember;

public class RemoveMemberValidator : AbstractValidator<RemoveMemberCommand>
{
    public RemoveMemberValidator()
    {
        RuleFor(x => x.ProjectId).NotEmpty().WithMessage(ValidationMessages.ProjectIdRequired);
        RuleFor(x => x.UserId).NotEmpty().WithMessage(ValidationMessages.MemberUserIdRequired);
    }
}

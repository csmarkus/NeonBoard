using FluentValidation;
using NeonBoard.Application.Common;

namespace NeonBoard.Application.Projects.Commands.UpdateMemberRole;

public class UpdateMemberRoleValidator : AbstractValidator<UpdateMemberRoleCommand>
{
    public UpdateMemberRoleValidator()
    {
        RuleFor(x => x.ProjectId).NotEmpty().WithMessage(ValidationMessages.ProjectIdRequired);
        RuleFor(x => x.UserId).NotEmpty().WithMessage(ValidationMessages.MemberUserIdRequired);
    }
}

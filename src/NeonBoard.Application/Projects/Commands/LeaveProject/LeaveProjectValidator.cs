using FluentValidation;
using NeonBoard.Application.Common;

namespace NeonBoard.Application.Projects.Commands.LeaveProject;

public class LeaveProjectValidator : AbstractValidator<LeaveProjectCommand>
{
    public LeaveProjectValidator()
    {
        RuleFor(x => x.ProjectId).NotEmpty().WithMessage(ValidationMessages.ProjectIdRequired);
        RuleFor(x => x.UserId).NotEmpty().WithMessage(ValidationMessages.MemberUserIdRequired);
    }
}

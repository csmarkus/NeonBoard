using FluentValidation;
using NeonBoard.Application.Common;

namespace NeonBoard.Application.Projects.Commands.UpdateProject;

public class UpdateProjectValidator : AbstractValidator<UpdateProjectCommand>
{
    public UpdateProjectValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotEmpty()
            .WithMessage(ValidationMessages.ProjectIdRequired);

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage(ValidationMessages.ProjectNameRequired)
            .MaximumLength(100)
            .WithMessage(ValidationMessages.ProjectNameTooLong);

        RuleFor(x => x.Description)
            .MaximumLength(1000)
            .WithMessage(ValidationMessages.ProjectDescriptionTooLong);
    }
}

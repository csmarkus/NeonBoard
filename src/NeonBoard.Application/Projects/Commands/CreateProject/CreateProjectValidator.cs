using FluentValidation;
using NeonBoard.Application.Common;

namespace NeonBoard.Application.Projects.Commands.CreateProject;

public class CreateProjectValidator : AbstractValidator<CreateProjectCommand>
{
    public CreateProjectValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage(ValidationMessages.ProjectNameRequired)
            .MaximumLength(100)
            .WithMessage(ValidationMessages.ProjectNameTooLong);

        RuleFor(x => x.Description)
            .MaximumLength(1000)
            .WithMessage(ValidationMessages.ProjectDescriptionTooLong);

        RuleFor(x => x.OwnerId)
            .NotEmpty()
            .WithMessage(ValidationMessages.OwnerIdRequired);
    }
}

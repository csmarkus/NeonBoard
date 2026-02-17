using FluentValidation;
using NeonBoard.Application.Common;

namespace NeonBoard.Application.Projects.Commands.DeleteProject;

public class DeleteProjectValidator : AbstractValidator<DeleteProjectCommand>
{
    public DeleteProjectValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotEmpty()
            .WithMessage(ValidationMessages.ProjectIdRequired);
    }
}

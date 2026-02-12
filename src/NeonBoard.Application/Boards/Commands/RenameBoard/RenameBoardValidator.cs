using FluentValidation;

namespace NeonBoard.Application.Boards.Commands.RenameBoard;

public class RenameBoardValidator : AbstractValidator<RenameBoardCommand>
{
    public RenameBoardValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotEmpty()
            .WithMessage("Project ID is required.");

        RuleFor(x => x.BoardId)
            .NotEmpty()
            .WithMessage("Board ID is required.");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Board name is required.")
            .MaximumLength(100)
            .WithMessage("Board name cannot exceed 100 characters.");
    }
}

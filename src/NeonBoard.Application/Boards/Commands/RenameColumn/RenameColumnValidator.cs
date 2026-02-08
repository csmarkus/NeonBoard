using FluentValidation;

namespace NeonBoard.Application.Boards.Commands.RenameColumn;

public class RenameColumnValidator : AbstractValidator<RenameColumnCommand>
{
    public RenameColumnValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotEmpty()
            .WithMessage("Project ID is required.");

        RuleFor(x => x.BoardId)
            .NotEmpty()
            .WithMessage("Board ID is required.");

        RuleFor(x => x.ColumnId)
            .NotEmpty()
            .WithMessage("Column ID is required.");

        RuleFor(x => x.NewName)
            .NotEmpty()
            .WithMessage("Column name is required.")
            .MaximumLength(100)
            .WithMessage("Column name cannot exceed 100 characters.");
    }
}

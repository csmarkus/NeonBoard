using FluentValidation;

namespace NeonBoard.Application.Columns.Commands.ReorderColumns;

public class ReorderColumnsValidator : AbstractValidator<ReorderColumnsCommand>
{
    public ReorderColumnsValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotEmpty()
            .WithMessage("Project ID is required.");

        RuleFor(x => x.BoardId)
            .NotEmpty()
            .WithMessage("Board ID is required.");

        RuleFor(x => x.ColumnIds)
            .NotEmpty()
            .WithMessage("Column IDs are required.");
    }
}

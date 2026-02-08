using FluentValidation;

namespace NeonBoard.Application.Columns.Commands.DeleteColumn;

public class DeleteColumnValidator : AbstractValidator<DeleteColumnCommand>
{
    public DeleteColumnValidator()
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
    }
}

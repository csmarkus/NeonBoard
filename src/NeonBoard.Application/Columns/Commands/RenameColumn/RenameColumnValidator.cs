using FluentValidation;
using NeonBoard.Application.Common;

namespace NeonBoard.Application.Columns.Commands.RenameColumn;

public class RenameColumnValidator : AbstractValidator<RenameColumnCommand>
{
    public RenameColumnValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotEmpty()
            .WithMessage(ValidationMessages.ProjectIdRequired);

        RuleFor(x => x.BoardId)
            .NotEmpty()
            .WithMessage(ValidationMessages.BoardIdRequired);

        RuleFor(x => x.ColumnId)
            .NotEmpty()
            .WithMessage(ValidationMessages.ColumnIdRequired);

        RuleFor(x => x.NewName)
            .NotEmpty()
            .WithMessage(ValidationMessages.ColumnNameRequired)
            .MaximumLength(100)
            .WithMessage(ValidationMessages.ColumnNameTooLong);
    }
}

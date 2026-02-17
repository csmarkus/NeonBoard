using FluentValidation;
using NeonBoard.Application.Common;

namespace NeonBoard.Application.Columns.Commands.AddColumn;

public class AddColumnValidator : AbstractValidator<AddColumnCommand>
{
    public AddColumnValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotEmpty()
            .WithMessage(ValidationMessages.ProjectIdRequired);

        RuleFor(x => x.BoardId)
            .NotEmpty()
            .WithMessage(ValidationMessages.BoardIdRequired);

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage(ValidationMessages.ColumnNameRequired)
            .MaximumLength(100)
            .WithMessage(ValidationMessages.ColumnNameTooLong);
    }
}

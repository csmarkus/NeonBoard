using FluentValidation;
using NeonBoard.Application.Common;

namespace NeonBoard.Application.Columns.Commands.DeleteColumn;

public class DeleteColumnValidator : AbstractValidator<DeleteColumnCommand>
{
    public DeleteColumnValidator()
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
    }
}

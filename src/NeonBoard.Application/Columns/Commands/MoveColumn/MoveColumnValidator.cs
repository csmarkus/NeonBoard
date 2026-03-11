using FluentValidation;
using NeonBoard.Application.Common;

namespace NeonBoard.Application.Columns.Commands.MoveColumn;

public class MoveColumnValidator : AbstractValidator<MoveColumnCommand>
{
    public MoveColumnValidator()
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

        RuleFor(x => x.NewPosition)
            .NotEmpty()
            .WithMessage(ValidationMessages.PositionRequired);
    }
}

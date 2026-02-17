using FluentValidation;
using NeonBoard.Application.Common;

namespace NeonBoard.Application.Columns.Commands.ReorderColumns;

public class ReorderColumnsValidator : AbstractValidator<ReorderColumnsCommand>
{
    public ReorderColumnsValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotEmpty()
            .WithMessage(ValidationMessages.ProjectIdRequired);

        RuleFor(x => x.BoardId)
            .NotEmpty()
            .WithMessage(ValidationMessages.BoardIdRequired);

        RuleFor(x => x.ColumnIds)
            .NotEmpty()
            .WithMessage(ValidationMessages.ColumnIdsRequired);
    }
}

using FluentValidation;
using NeonBoard.Application.Common;

namespace NeonBoard.Application.Cards.Commands.MoveCard;

public class MoveCardValidator : AbstractValidator<MoveCardCommand>
{
    public MoveCardValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotEmpty()
            .WithMessage(ValidationMessages.ProjectIdRequired);

        RuleFor(x => x.BoardId)
            .NotEmpty()
            .WithMessage(ValidationMessages.BoardIdRequired);

        RuleFor(x => x.CardId)
            .NotEmpty()
            .WithMessage(ValidationMessages.CardIdRequired);

        RuleFor(x => x.TargetColumnId)
            .NotEmpty()
            .WithMessage(ValidationMessages.TargetColumnIdRequired);

        RuleFor(x => x.TargetPosition)
            .GreaterThanOrEqualTo(0)
            .WithMessage(ValidationMessages.TargetPositionNonNegative);
    }
}

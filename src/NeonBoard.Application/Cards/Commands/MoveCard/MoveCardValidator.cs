using FluentValidation;

namespace NeonBoard.Application.Cards.Commands.MoveCard;

public class MoveCardValidator : AbstractValidator<MoveCardCommand>
{
    public MoveCardValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotEmpty()
            .WithMessage("Project ID is required.");

        RuleFor(x => x.BoardId)
            .NotEmpty()
            .WithMessage("Board ID is required.");

        RuleFor(x => x.CardId)
            .NotEmpty()
            .WithMessage("Card ID is required.");

        RuleFor(x => x.TargetColumnId)
            .NotEmpty()
            .WithMessage("Target column ID is required.");

        RuleFor(x => x.TargetPosition)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Target position must be greater than or equal to 0.");
    }
}

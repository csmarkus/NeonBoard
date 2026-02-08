using FluentValidation;

namespace NeonBoard.Application.Cards.Commands.DeleteCard;

public class DeleteCardValidator : AbstractValidator<DeleteCardCommand>
{
    public DeleteCardValidator()
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
    }
}

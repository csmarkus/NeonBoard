using FluentValidation;

namespace NeonBoard.Application.Cards.Commands.UpdateCard;

public class UpdateCardValidator : AbstractValidator<UpdateCardCommand>
{
    public UpdateCardValidator()
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

        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("Card title is required.")
            .MaximumLength(200)
            .WithMessage("Card title cannot exceed 200 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(2000)
            .WithMessage("Card description cannot exceed 2000 characters.");
    }
}

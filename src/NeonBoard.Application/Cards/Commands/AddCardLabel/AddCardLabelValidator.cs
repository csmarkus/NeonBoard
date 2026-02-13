using FluentValidation;

namespace NeonBoard.Application.Cards.Commands.AddCardLabel;

public class AddCardLabelValidator : AbstractValidator<AddCardLabelCommand>
{
    public AddCardLabelValidator()
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

        RuleFor(x => x.LabelId)
            .NotEmpty()
            .WithMessage("Label ID is required.");
    }
}

using FluentValidation;
using NeonBoard.Application.Common;

namespace NeonBoard.Application.Cards.Commands.UpdateCard;

public class UpdateCardValidator : AbstractValidator<UpdateCardCommand>
{
    public UpdateCardValidator()
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

        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage(ValidationMessages.CardTitleRequired)
            .MaximumLength(200)
            .WithMessage(ValidationMessages.CardTitleTooLong);

        RuleFor(x => x.Description)
            .MaximumLength(2000)
            .WithMessage(ValidationMessages.CardDescriptionTooLong);
    }
}

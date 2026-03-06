using FluentValidation;
using NeonBoard.Application.Common;

namespace NeonBoard.Application.Cards.Commands.AddCard;

public class AddCardValidator : AbstractValidator<AddCardCommand>
{
    public AddCardValidator()
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

        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage(ValidationMessages.CardTitleRequired)
            .MaximumLength(200)
            .WithMessage(ValidationMessages.CardTitleTooLong);

        RuleFor(x => x.Description)
            .MaximumLength(5000)
            .WithMessage(ValidationMessages.CardDescriptionTooLong);
    }
}

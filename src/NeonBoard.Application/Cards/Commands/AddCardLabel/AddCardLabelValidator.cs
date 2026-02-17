using FluentValidation;
using NeonBoard.Application.Common;

namespace NeonBoard.Application.Cards.Commands.AddCardLabel;

public class AddCardLabelValidator : AbstractValidator<AddCardLabelCommand>
{
    public AddCardLabelValidator()
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

        RuleFor(x => x.LabelId)
            .NotEmpty()
            .WithMessage(ValidationMessages.LabelIdRequired);
    }
}

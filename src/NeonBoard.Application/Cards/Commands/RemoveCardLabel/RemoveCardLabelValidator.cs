using FluentValidation;
using NeonBoard.Application.Common;

namespace NeonBoard.Application.Cards.Commands.RemoveCardLabel;

public class RemoveCardLabelValidator : AbstractValidator<RemoveCardLabelCommand>
{
    public RemoveCardLabelValidator()
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

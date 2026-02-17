using FluentValidation;
using NeonBoard.Application.Common;

namespace NeonBoard.Application.Cards.Commands.DeleteCard;

public class DeleteCardValidator : AbstractValidator<DeleteCardCommand>
{
    public DeleteCardValidator()
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
    }
}

using FluentValidation;
using NeonBoard.Application.Common;

namespace NeonBoard.Application.Labels.Commands.RemoveLabel;

public class RemoveLabelValidator : AbstractValidator<RemoveLabelCommand>
{
    public RemoveLabelValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotEmpty()
            .WithMessage(ValidationMessages.ProjectIdRequired);

        RuleFor(x => x.BoardId)
            .NotEmpty()
            .WithMessage(ValidationMessages.BoardIdRequired);

        RuleFor(x => x.LabelId)
            .NotEmpty()
            .WithMessage(ValidationMessages.LabelIdRequired);
    }
}

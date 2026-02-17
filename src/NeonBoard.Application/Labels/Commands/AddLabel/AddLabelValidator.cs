using FluentValidation;
using NeonBoard.Application.Common;

namespace NeonBoard.Application.Labels.Commands.AddLabel;

public class AddLabelValidator : AbstractValidator<AddLabelCommand>
{
    public AddLabelValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotEmpty()
            .WithMessage(ValidationMessages.ProjectIdRequired);

        RuleFor(x => x.BoardId)
            .NotEmpty()
            .WithMessage(ValidationMessages.BoardIdRequired);

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage(ValidationMessages.LabelNameRequired)
            .MaximumLength(50)
            .WithMessage(ValidationMessages.LabelNameTooLong);

        RuleFor(x => x.Color)
            .NotEmpty()
            .WithMessage(ValidationMessages.LabelColorRequired);
    }
}

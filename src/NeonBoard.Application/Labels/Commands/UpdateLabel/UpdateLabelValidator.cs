using FluentValidation;
using NeonBoard.Application.Common;

namespace NeonBoard.Application.Labels.Commands.UpdateLabel;

public class UpdateLabelValidator : AbstractValidator<UpdateLabelCommand>
{
    public UpdateLabelValidator()
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

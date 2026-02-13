using FluentValidation;

namespace NeonBoard.Application.Labels.Commands.UpdateLabel;

public class UpdateLabelValidator : AbstractValidator<UpdateLabelCommand>
{
    public UpdateLabelValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotEmpty()
            .WithMessage("Project ID is required.");

        RuleFor(x => x.BoardId)
            .NotEmpty()
            .WithMessage("Board ID is required.");

        RuleFor(x => x.LabelId)
            .NotEmpty()
            .WithMessage("Label ID is required.");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Label name is required.")
            .MaximumLength(50)
            .WithMessage("Label name cannot exceed 50 characters.");

        RuleFor(x => x.Color)
            .NotEmpty()
            .WithMessage("Label color is required.");
    }
}

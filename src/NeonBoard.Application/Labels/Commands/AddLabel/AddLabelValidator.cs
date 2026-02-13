using FluentValidation;

namespace NeonBoard.Application.Labels.Commands.AddLabel;

public class AddLabelValidator : AbstractValidator<AddLabelCommand>
{
    public AddLabelValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotEmpty()
            .WithMessage("Project ID is required.");

        RuleFor(x => x.BoardId)
            .NotEmpty()
            .WithMessage("Board ID is required.");

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

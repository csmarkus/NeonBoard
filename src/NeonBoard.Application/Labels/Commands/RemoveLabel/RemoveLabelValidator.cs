using FluentValidation;

namespace NeonBoard.Application.Labels.Commands.RemoveLabel;

public class RemoveLabelValidator : AbstractValidator<RemoveLabelCommand>
{
    public RemoveLabelValidator()
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
    }
}

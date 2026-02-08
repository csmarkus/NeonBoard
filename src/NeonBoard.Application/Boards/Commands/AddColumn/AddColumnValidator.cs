using FluentValidation;

namespace NeonBoard.Application.Boards.Commands.AddColumn;

public class AddColumnValidator : AbstractValidator<AddColumnCommand>
{
    public AddColumnValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotEmpty()
            .WithMessage("Project ID is required.");

        RuleFor(x => x.BoardId)
            .NotEmpty()
            .WithMessage("Board ID is required.");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Column name is required.")
            .MaximumLength(100)
            .WithMessage("Column name cannot exceed 100 characters.");
    }
}

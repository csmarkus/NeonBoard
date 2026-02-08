using FluentValidation;

namespace NeonBoard.Application.Boards.Commands.CreateBoard;

public class CreateBoardValidator : AbstractValidator<CreateBoardCommand>
{
    public CreateBoardValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotEmpty()
            .WithMessage("Project ID is required.");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Board name is required.")
            .MaximumLength(100)
            .WithMessage("Board name cannot exceed 100 characters.");
    }
}

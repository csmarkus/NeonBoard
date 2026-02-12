using FluentValidation;

namespace NeonBoard.Application.Boards.Commands.DeleteBoard;

public class DeleteBoardValidator : AbstractValidator<DeleteBoardCommand>
{
    public DeleteBoardValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotEmpty()
            .WithMessage("Project ID is required.");

        RuleFor(x => x.BoardId)
            .NotEmpty()
            .WithMessage("Board ID is required.");
    }
}

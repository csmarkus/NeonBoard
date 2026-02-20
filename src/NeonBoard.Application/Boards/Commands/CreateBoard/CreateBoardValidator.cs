using FluentValidation;
using NeonBoard.Application.Common;

namespace NeonBoard.Application.Boards.Commands.CreateBoard;

public class CreateBoardValidator : AbstractValidator<CreateBoardCommand>
{
    public CreateBoardValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotEmpty()
            .WithMessage(ValidationMessages.ProjectIdRequired);

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage(ValidationMessages.BoardNameRequired)
            .MaximumLength(100)
            .WithMessage(ValidationMessages.BoardNameTooLong);

        RuleFor(x => x.Prefix)
            .Matches(@"^[A-Z]{2,5}$")
            .When(x => x.Prefix != null)
            .WithMessage(ValidationMessages.BoardPrefixInvalid);
    }
}

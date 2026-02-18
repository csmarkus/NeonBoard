using NeonBoard.Application.Boards.Commands.CreateBoard;
using NeonBoard.Application.Common;

namespace NeonBoard.UnitTests.Application.Boards.Commands;

public class CreateBoardValidatorTests
{
    private readonly CreateBoardValidator _validator = new();

    [Fact]
    public void Validate_WithValidData_ShouldPass()
    {
        var command = new CreateBoardCommand(Guid.NewGuid(), "Test Board");

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithEmptyName_ShouldFail()
    {
        var command = new CreateBoardCommand(Guid.NewGuid(), "");

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == ValidationMessages.BoardNameRequired);
    }

    [Fact]
    public void Validate_WithNameExceedingMaxLength_ShouldFail()
    {
        var command = new CreateBoardCommand(Guid.NewGuid(), new string('a', 101));

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == ValidationMessages.BoardNameTooLong);
    }

    [Fact]
    public void Validate_WithEmptyProjectId_ShouldFail()
    {
        var command = new CreateBoardCommand(Guid.Empty, "Test Board");

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == ValidationMessages.ProjectIdRequired);
    }
}

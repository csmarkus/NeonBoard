using NeonBoard.Application.Boards.Commands.UpdateBoardSettings;
using NeonBoard.Application.Common.Exceptions;
using NeonBoard.Application.Common.Interfaces;
using NeonBoard.Domain.Boards;
using NSubstitute;

namespace NeonBoard.UnitTests.Application.Boards.Commands;

public class UpdateBoardSettingsHandlerTests
{
    private readonly IBoardRepository _boardRepository = Substitute.For<IBoardRepository>();
    private readonly UpdateBoardSettingsHandler _handler;

    public UpdateBoardSettingsHandlerTests()
    {
        _handler = new UpdateBoardSettingsHandler(_boardRepository);
    }

    [Fact]
    public async Task Handle_WithNewPrefix_ShouldUpdatePrefix()
    {
        var board = Board.Create("Test Board", Guid.NewGuid(), "OLD");
        _boardRepository.GetBoardWithDetailsAsync(board.Id, Arg.Any<CancellationToken>())
            .Returns(board);
        _boardRepository.PrefixExistsInProjectAsync(
            board.ProjectId, "NEW", board.Id, Arg.Any<CancellationToken>())
            .Returns(false);

        var command = new UpdateBoardSettingsCommand(board.ProjectId, board.Id, "Test Board", "NEW");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Prefix.Should().Be("NEW");
    }

    [Fact]
    public async Task Handle_WithDuplicatePrefix_ShouldThrowConflictException()
    {
        var board = Board.Create("Test Board", Guid.NewGuid(), "OLD");
        _boardRepository.GetBoardWithDetailsAsync(board.Id, Arg.Any<CancellationToken>())
            .Returns(board);
        _boardRepository.PrefixExistsInProjectAsync(
            board.ProjectId, "DUP", board.Id, Arg.Any<CancellationToken>())
            .Returns(true);

        var command = new UpdateBoardSettingsCommand(board.ProjectId, board.Id, "Test Board", "DUP");

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task Handle_WhenBoardNotFound_ShouldThrowNotFoundException()
    {
        var boardId = Guid.NewGuid();
        _boardRepository.GetBoardWithDetailsAsync(boardId, Arg.Any<CancellationToken>())
            .Returns((Board?)null);

        var command = new UpdateBoardSettingsCommand(Guid.NewGuid(), boardId, "Test Board", "PFX");

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WithNewName_ShouldUpdateName()
    {
        var board = Board.Create("Old Name", Guid.NewGuid(), "TST");
        _boardRepository.GetBoardWithDetailsAsync(board.Id, Arg.Any<CancellationToken>())
            .Returns(board);

        var command = new UpdateBoardSettingsCommand(board.ProjectId, board.Id, "New Name");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Name.Should().Be("New Name");
        result.Prefix.Should().Be("TST");
    }

    [Fact]
    public async Task Handle_WithSamePrefix_ShouldNotCheckForDuplicates()
    {
        var board = Board.Create("Test Board", Guid.NewGuid(), "SAME");
        _boardRepository.GetBoardWithDetailsAsync(board.Id, Arg.Any<CancellationToken>())
            .Returns(board);

        var command = new UpdateBoardSettingsCommand(board.ProjectId, board.Id, "Test Board", "SAME");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Prefix.Should().Be("SAME");
        await _boardRepository.DidNotReceive().PrefixExistsInProjectAsync(
            Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<Guid?>(), Arg.Any<CancellationToken>());
    }
}

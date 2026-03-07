using NeonBoard.Application.Boards.Commands.DeleteBoard;
using NeonBoard.Application.Common.Exceptions;
using NeonBoard.Application.Common.Interfaces;
using NeonBoard.Domain.Boards;
using NSubstitute;

namespace NeonBoard.UnitTests.Application.Boards.Commands;

public class DeleteBoardHandlerTests
{
    private readonly IBoardRepository _boardRepository = Substitute.For<IBoardRepository>();
    private readonly DeleteBoardHandler _handler;

    public DeleteBoardHandlerTests()
    {
        _handler = new DeleteBoardHandler(_boardRepository);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldReturnUnit()
    {
        var board = Board.Create("Test Board", Guid.NewGuid());
        _boardRepository.GetByIdAsync(board.Id, Arg.Any<CancellationToken>())
            .Returns(board);

        var command = new DeleteBoardCommand(board.ProjectId, board.Id);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().Be(MediatR.Unit.Value);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCallDeleteAsync()
    {
        var board = Board.Create("Test Board", Guid.NewGuid());
        _boardRepository.GetByIdAsync(board.Id, Arg.Any<CancellationToken>())
            .Returns(board);

        var command = new DeleteBoardCommand(board.ProjectId, board.Id);

        await _handler.Handle(command, CancellationToken.None);

        await _boardRepository.Received(1).DeleteAsync(
            Arg.Is<Board>(b => b.Id == board.Id),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenBoardNotFound_ShouldThrowNotFoundException()
    {
        var boardId = Guid.NewGuid();
        _boardRepository.GetByIdAsync(boardId, Arg.Any<CancellationToken>())
            .Returns((Board?)null);

        var command = new DeleteBoardCommand(Guid.NewGuid(), boardId);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}

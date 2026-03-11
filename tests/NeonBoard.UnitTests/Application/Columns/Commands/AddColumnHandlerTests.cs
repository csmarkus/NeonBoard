using NeonBoard.Application.Columns.Commands.AddColumn;
using NeonBoard.Application.Common.Exceptions;
using NeonBoard.Application.Common.Interfaces;
using NeonBoard.Domain.Boards;
using NeonBoard.UnitTests.Builders;
using NSubstitute;

namespace NeonBoard.UnitTests.Application.Columns.Commands;

public class AddColumnHandlerTests
{
    private readonly IBoardRepository _boardRepository = Substitute.For<IBoardRepository>();
    private readonly AddColumnHandler _handler;

    public AddColumnHandlerTests()
    {
        _handler = new AddColumnHandler(_boardRepository);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldReturnColumnDto()
    {
        var projectId = Guid.NewGuid();
        var board = new BoardBuilder()
            .WithProjectId(projectId)
            .Build();

        _boardRepository.GetBoardWithDetailsAsync(board.Id, Arg.Any<CancellationToken>())
            .Returns(board);

        var command = new AddColumnCommand(projectId, board.Id, "New Column");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Name.Should().Be("New Column");
        result.BoardId.Should().Be(board.Id);
        result.Id.Should().NotBeEmpty();
        result.Position.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Handle_WithExistingColumns_ShouldReturnCorrectPosition()
    {
        var projectId = Guid.NewGuid();
        var board = new BoardBuilder()
            .WithProjectId(projectId)
            .WithColumns("Column 1", "Column 2")
            .Build();

        _boardRepository.GetBoardWithDetailsAsync(board.Id, Arg.Any<CancellationToken>())
            .Returns(board);

        var command = new AddColumnCommand(projectId, board.Id, "Column 3");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Name.Should().Be("Column 3");
        result.Position.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Handle_WhenBoardNotFound_ShouldThrowNotFoundException()
    {
        var boardId = Guid.NewGuid();
        _boardRepository.GetBoardWithDetailsAsync(boardId, Arg.Any<CancellationToken>())
            .Returns((Board?)null);

        var command = new AddColumnCommand(Guid.NewGuid(), boardId, "Column");

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenProjectIdMismatch_ShouldThrowNotFoundException()
    {
        var board = new BoardBuilder()
            .WithProjectId(Guid.NewGuid())
            .Build();

        _boardRepository.GetBoardWithDetailsAsync(board.Id, Arg.Any<CancellationToken>())
            .Returns(board);

        var differentProjectId = Guid.NewGuid();
        var command = new AddColumnCommand(differentProjectId, board.Id, "Column");

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}

using NeonBoard.Application.Columns.Commands.RenameColumn;
using NeonBoard.Application.Common.Exceptions;
using NeonBoard.Application.Common.Interfaces;
using NeonBoard.Domain.Boards;
using NeonBoard.UnitTests.Builders;
using NSubstitute;

namespace NeonBoard.UnitTests.Application.Columns.Commands;

public class RenameColumnHandlerTests
{
    private readonly IBoardRepository _boardRepository = Substitute.For<IBoardRepository>();
    private readonly RenameColumnHandler _handler;

    public RenameColumnHandlerTests()
    {
        _handler = new RenameColumnHandler(_boardRepository);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldReturnUnit()
    {
        var projectId = Guid.NewGuid();
        var board = new BoardBuilder()
            .WithProjectId(projectId)
            .WithColumns("Old Name")
            .Build();

        var columnId = board.Columns[0].Id;
        _boardRepository.GetBoardWithDetailsAsync(board.Id, Arg.Any<CancellationToken>())
            .Returns(board);

        var command = new RenameColumnCommand(projectId, board.Id, columnId, "New Name");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().Be(MediatR.Unit.Value);
    }

    [Fact]
    public async Task Handle_WhenBoardNotFound_ShouldThrowNotFoundException()
    {
        var boardId = Guid.NewGuid();
        _boardRepository.GetBoardWithDetailsAsync(boardId, Arg.Any<CancellationToken>())
            .Returns((Board?)null);

        var command = new RenameColumnCommand(Guid.NewGuid(), boardId, Guid.NewGuid(), "New Name");

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenProjectIdMismatch_ShouldThrowNotFoundException()
    {
        var board = new BoardBuilder()
            .WithProjectId(Guid.NewGuid())
            .WithColumns("Column")
            .Build();

        _boardRepository.GetBoardWithDetailsAsync(board.Id, Arg.Any<CancellationToken>())
            .Returns(board);

        var differentProjectId = Guid.NewGuid();
        var command = new RenameColumnCommand(differentProjectId, board.Id, board.Columns[0].Id, "New Name");

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}

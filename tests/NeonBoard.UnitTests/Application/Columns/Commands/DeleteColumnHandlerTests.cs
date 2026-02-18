using NeonBoard.Application.Columns.Commands.DeleteColumn;
using NeonBoard.Application.Common.Exceptions;
using NeonBoard.Application.Common.Interfaces;
using NeonBoard.Domain.Boards;
using NeonBoard.Domain.Common;
using NeonBoard.UnitTests.Builders;
using NSubstitute;

namespace NeonBoard.UnitTests.Application.Columns.Commands;

public class DeleteColumnHandlerTests
{
    private readonly IBoardRepository _boardRepository = Substitute.For<IBoardRepository>();
    private readonly DeleteColumnHandler _handler;

    public DeleteColumnHandlerTests()
    {
        _handler = new DeleteColumnHandler(_boardRepository);
    }

    [Fact]
    public async Task Handle_WithEmptyColumn_ShouldDeleteColumn()
    {
        var projectId = Guid.NewGuid();
        var board = new BoardBuilder()
            .WithProjectId(projectId)
            .WithColumns("To Do", "Done")
            .Build();

        var columnId = board.Columns.First(c => c.Name == "To Do").Id;

        _boardRepository.GetBoardWithDetailsAsync(board.Id, Arg.Any<CancellationToken>())
            .Returns(board);

        var command = new DeleteColumnCommand(projectId, board.Id, columnId);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Handle_WithCardsInColumn_ShouldThrowDomainException()
    {
        var projectId = Guid.NewGuid();
        var board = new BoardBuilder()
            .WithProjectId(projectId)
            .WithColumns("To Do")
            .WithCard("To Do", "Card 1")
            .Build();

        var columnId = board.Columns[0].Id;

        _boardRepository.GetBoardWithDetailsAsync(board.Id, Arg.Any<CancellationToken>())
            .Returns(board);

        var command = new DeleteColumnCommand(projectId, board.Id, columnId);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*Cannot delete column*");
    }

    [Fact]
    public async Task Handle_WhenBoardNotFound_ShouldThrowNotFoundException()
    {
        var boardId = Guid.NewGuid();
        _boardRepository.GetBoardWithDetailsAsync(boardId, Arg.Any<CancellationToken>())
            .Returns((Board?)null);

        var command = new DeleteColumnCommand(Guid.NewGuid(), boardId, Guid.NewGuid());

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}

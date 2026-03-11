using NeonBoard.Application.Cards.Commands.MoveCard;
using NeonBoard.Application.Common.Exceptions;
using NeonBoard.Application.Common.Interfaces;
using NeonBoard.Domain.Boards;
using NeonBoard.Domain.Common;
using NeonBoard.UnitTests.Builders;
using NSubstitute;

namespace NeonBoard.UnitTests.Application.Cards.Commands;

public class MoveCardHandlerTests
{
    private readonly IBoardRepository _boardRepository = Substitute.For<IBoardRepository>();
    private readonly MoveCardHandler _handler;

    public MoveCardHandlerTests()
    {
        _handler = new MoveCardHandler(_boardRepository);
    }

    [Fact]
    public async Task Handle_WithValidMove_ShouldCallMoveCardOnBoard()
    {
        var projectId = Guid.NewGuid();
        var board = new BoardBuilder()
            .WithProjectId(projectId)
            .WithColumns("To Do", "In Progress")
            .WithCard("To Do", "Card 1")
            .Build();

        var cardId = board.Cards[0].Id;
        var targetColumnId = board.Columns.First(c => c.Name == "In Progress").Id;

        _boardRepository.GetBoardWithDetailsAsync(board.Id, Arg.Any<CancellationToken>())
            .Returns(board);

        var newPosition = FractionalIndex.GenerateKeyBetween(null, null);
        var command = new MoveCardCommand(projectId, board.Id, cardId, targetColumnId, newPosition);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().Be(MediatR.Unit.Value);
    }

    [Fact]
    public async Task Handle_WhenBoardNotFound_ShouldThrowNotFoundException()
    {
        var boardId = Guid.NewGuid();
        _boardRepository.GetBoardWithDetailsAsync(boardId, Arg.Any<CancellationToken>())
            .Returns((Board?)null);

        var newPosition = FractionalIndex.GenerateKeyBetween(null, null);
        var command = new MoveCardCommand(Guid.NewGuid(), boardId, Guid.NewGuid(), Guid.NewGuid(), newPosition);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenProjectIdMismatch_ShouldThrowNotFoundException()
    {
        var board = new BoardBuilder()
            .WithProjectId(Guid.NewGuid())
            .WithColumns("To Do")
            .WithCard("To Do", "Card 1")
            .Build();

        _boardRepository.GetBoardWithDetailsAsync(board.Id, Arg.Any<CancellationToken>())
            .Returns(board);

        var differentProjectId = Guid.NewGuid();
        var newPosition = FractionalIndex.GenerateKeyBetween(null, null);
        var command = new MoveCardCommand(differentProjectId, board.Id, board.Cards[0].Id, board.Columns[0].Id, newPosition);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}

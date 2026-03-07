using NeonBoard.Application.Cards.Commands.DeleteCard;
using NeonBoard.Application.Common.Exceptions;
using NeonBoard.Application.Common.Interfaces;
using NeonBoard.Domain.Boards;
using NeonBoard.UnitTests.Builders;
using NSubstitute;

namespace NeonBoard.UnitTests.Application.Cards.Commands;

public class DeleteCardHandlerTests
{
    private readonly IBoardRepository _boardRepository = Substitute.For<IBoardRepository>();
    private readonly DeleteCardHandler _handler;

    public DeleteCardHandlerTests()
    {
        _handler = new DeleteCardHandler(_boardRepository);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldReturnUnit()
    {
        var projectId = Guid.NewGuid();
        var board = new BoardBuilder()
            .WithProjectId(projectId)
            .WithColumns("To Do")
            .WithCard("To Do", "Card 1")
            .Build();

        var cardId = board.Cards[0].Id;
        _boardRepository.GetBoardWithDetailsAsync(board.Id, Arg.Any<CancellationToken>())
            .Returns(board);

        var command = new DeleteCardCommand(projectId, board.Id, cardId);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().Be(MediatR.Unit.Value);
    }

    [Fact]
    public async Task Handle_WhenBoardNotFound_ShouldThrowNotFoundException()
    {
        var boardId = Guid.NewGuid();
        _boardRepository.GetBoardWithDetailsAsync(boardId, Arg.Any<CancellationToken>())
            .Returns((Board?)null);

        var command = new DeleteCardCommand(Guid.NewGuid(), boardId, Guid.NewGuid());

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
        var command = new DeleteCardCommand(differentProjectId, board.Id, board.Cards[0].Id);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}

using NeonBoard.Application.Cards.Commands.HoldCard;
using NeonBoard.Application.Common.Exceptions;
using NeonBoard.Application.Common.Interfaces;
using NeonBoard.Domain.Boards;
using NeonBoard.UnitTests.Builders;
using NSubstitute;

namespace NeonBoard.UnitTests.Application.Cards.Commands;

public class HoldCardHandlerTests
{
    private readonly IBoardRepository _boardRepository = Substitute.For<IBoardRepository>();
    private readonly HoldCardHandler _handler;

    public HoldCardHandlerTests()
    {
        _handler = new HoldCardHandler(_boardRepository);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldReturnCardDtoWithHoldState()
    {
        var projectId = Guid.NewGuid();
        var board = new BoardBuilder()
            .WithProjectId(projectId)
            .WithColumns("To Do")
            .WithCard("To Do", "Card 1", "Description")
            .Build();

        var cardId = board.Cards[0].Id;
        _boardRepository.GetBoardWithDetailsAsync(board.Id, Arg.Any<CancellationToken>())
            .Returns(board);

        var command = new HoldCardCommand(projectId, board.Id, cardId);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Id.Should().Be(cardId);
        result.Title.Should().Be("Card 1");
        result.HoldAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_WhenBoardNotFound_ShouldThrowNotFoundException()
    {
        var boardId = Guid.NewGuid();
        _boardRepository.GetBoardWithDetailsAsync(boardId, Arg.Any<CancellationToken>())
            .Returns((Board?)null);

        var command = new HoldCardCommand(Guid.NewGuid(), boardId, Guid.NewGuid());

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
        var command = new HoldCardCommand(differentProjectId, board.Id, board.Cards[0].Id);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}

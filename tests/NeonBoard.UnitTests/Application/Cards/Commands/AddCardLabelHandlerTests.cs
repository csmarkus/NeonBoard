using NeonBoard.Application.Cards.Commands.AddCardLabel;
using NeonBoard.Application.Common.Exceptions;
using NeonBoard.Application.Common.Interfaces;
using NeonBoard.Domain.Boards;
using NeonBoard.UnitTests.Builders;
using NSubstitute;

namespace NeonBoard.UnitTests.Application.Cards.Commands;

public class AddCardLabelHandlerTests
{
    private readonly IBoardRepository _boardRepository = Substitute.For<IBoardRepository>();
    private readonly AddCardLabelHandler _handler;

    public AddCardLabelHandlerTests()
    {
        _handler = new AddCardLabelHandler(_boardRepository);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldReturnUnit()
    {
        var projectId = Guid.NewGuid();
        var board = new BoardBuilder()
            .WithProjectId(projectId)
            .WithColumns("To Do")
            .WithCard("To Do", "Card 1")
            .WithLabel("Bug", LabelColors.Red)
            .Build();

        var cardId = board.Cards[0].Id;
        var labelId = board.Labels[0].Id;
        _boardRepository.GetBoardWithDetailsAsync(board.Id, Arg.Any<CancellationToken>())
            .Returns(board);

        var command = new AddCardLabelCommand(projectId, board.Id, cardId, labelId);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().Be(MediatR.Unit.Value);
    }

    [Fact]
    public async Task Handle_WhenBoardNotFound_ShouldThrowNotFoundException()
    {
        var boardId = Guid.NewGuid();
        _boardRepository.GetBoardWithDetailsAsync(boardId, Arg.Any<CancellationToken>())
            .Returns((Board?)null);

        var command = new AddCardLabelCommand(Guid.NewGuid(), boardId, Guid.NewGuid(), Guid.NewGuid());

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
            .WithLabel("Bug", LabelColors.Red)
            .Build();

        _boardRepository.GetBoardWithDetailsAsync(board.Id, Arg.Any<CancellationToken>())
            .Returns(board);

        var differentProjectId = Guid.NewGuid();
        var command = new AddCardLabelCommand(differentProjectId, board.Id, board.Cards[0].Id, board.Labels[0].Id);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}

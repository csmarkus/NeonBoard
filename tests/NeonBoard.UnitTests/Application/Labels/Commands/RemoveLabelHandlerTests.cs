using NeonBoard.Application.Common.Exceptions;
using NeonBoard.Application.Common.Interfaces;
using NeonBoard.Application.Labels.Commands.RemoveLabel;
using NeonBoard.Domain.Boards;
using NeonBoard.Domain.Boards.ValueObjects;
using NeonBoard.UnitTests.Builders;
using NSubstitute;

namespace NeonBoard.UnitTests.Application.Labels.Commands;

public class RemoveLabelHandlerTests
{
    private readonly IBoardRepository _boardRepository = Substitute.For<IBoardRepository>();
    private readonly RemoveLabelHandler _handler;

    public RemoveLabelHandlerTests()
    {
        _handler = new RemoveLabelHandler(_boardRepository);
    }

    [Fact]
    public async Task Handle_ShouldRemoveLabelFromBoard()
    {
        var projectId = Guid.NewGuid();
        var board = new BoardBuilder()
            .WithProjectId(projectId)
            .WithLabel("Bug", LabelColors.Red)
            .Build();

        var labelId = board.Labels[0].Id;

        _boardRepository.GetBoardWithDetailsAsync(board.Id, Arg.Any<CancellationToken>())
            .Returns(board);

        var command = new RemoveLabelCommand(projectId, board.Id, labelId);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().NotThrowAsync();
        board.Labels.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenBoardNotFound_ShouldThrowNotFoundException()
    {
        var boardId = Guid.NewGuid();
        _boardRepository.GetBoardWithDetailsAsync(boardId, Arg.Any<CancellationToken>())
            .Returns((Board?)null);

        var command = new RemoveLabelCommand(Guid.NewGuid(), boardId, Guid.NewGuid());

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenProjectIdMismatch_ShouldThrowNotFoundException()
    {
        var projectId = Guid.NewGuid();
        var board = new BoardBuilder()
            .WithProjectId(projectId)
            .WithLabel("Bug", LabelColors.Red)
            .Build();

        _boardRepository.GetBoardWithDetailsAsync(board.Id, Arg.Any<CancellationToken>())
            .Returns(board);

        var differentProjectId = Guid.NewGuid();
        var command = new RemoveLabelCommand(differentProjectId, board.Id, board.Labels[0].Id);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}

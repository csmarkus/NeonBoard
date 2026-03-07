using NeonBoard.Application.Cards.Commands.AddCard;
using NeonBoard.Application.Common.Exceptions;
using NeonBoard.Application.Common.Interfaces;
using NeonBoard.Domain.Boards;
using NeonBoard.UnitTests.Builders;
using NSubstitute;

namespace NeonBoard.UnitTests.Application.Cards.Commands;

public class AddCardHandlerTests
{
    private readonly IBoardRepository _boardRepository = Substitute.For<IBoardRepository>();
    private readonly AddCardHandler _handler;

    public AddCardHandlerTests()
    {
        _handler = new AddCardHandler(_boardRepository);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldReturnCardDto()
    {
        var projectId = Guid.NewGuid();
        var board = new BoardBuilder()
            .WithProjectId(projectId)
            .WithColumns("To Do")
            .WithLabel("Bug", LabelColors.Red)
            .Build();

        var columnId = board.Columns[0].Id;
        _boardRepository.GetBoardWithDetailsAsync(board.Id, Arg.Any<CancellationToken>())
            .Returns(board);

        var command = new AddCardCommand(projectId, board.Id, columnId, "New Card", "Description");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Title.Should().Be("New Card");
        result.Description.Should().Be("Description");
        result.ColumnId.Should().Be(columnId);
        result.Id.Should().NotBeEmpty();
        result.DisplayId.Should().Contain(board.Prefix.Value);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldIncludeLabels()
    {
        var projectId = Guid.NewGuid();
        var board = new BoardBuilder()
            .WithProjectId(projectId)
            .WithColumns("To Do")
            .WithLabel("Bug", LabelColors.Red)
            .Build();

        var columnId = board.Columns[0].Id;
        _boardRepository.GetBoardWithDetailsAsync(board.Id, Arg.Any<CancellationToken>())
            .Returns(board);

        var command = new AddCardCommand(projectId, board.Id, columnId, "New Card", "Description");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Labels.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_WhenBoardNotFound_ShouldThrowNotFoundException()
    {
        var boardId = Guid.NewGuid();
        _boardRepository.GetBoardWithDetailsAsync(boardId, Arg.Any<CancellationToken>())
            .Returns((Board?)null);

        var command = new AddCardCommand(Guid.NewGuid(), boardId, Guid.NewGuid(), "Card", "Desc");

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenProjectIdMismatch_ShouldThrowNotFoundException()
    {
        var board = new BoardBuilder()
            .WithProjectId(Guid.NewGuid())
            .WithColumns("To Do")
            .Build();

        _boardRepository.GetBoardWithDetailsAsync(board.Id, Arg.Any<CancellationToken>())
            .Returns(board);

        var differentProjectId = Guid.NewGuid();
        var command = new AddCardCommand(differentProjectId, board.Id, board.Columns[0].Id, "Card", "Desc");

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}

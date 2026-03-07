using NeonBoard.Application.Cards.Queries.GetCardDetail;
using NeonBoard.Application.Common.Exceptions;
using NeonBoard.Application.Common.Interfaces;
using NeonBoard.Domain.Boards;
using NeonBoard.Domain.Boards.Activity;
using NeonBoard.Domain.Boards.Entities;
using NeonBoard.UnitTests.Builders;
using NSubstitute;

namespace NeonBoard.UnitTests.Application.Cards.Queries;

public class GetCardDetailHandlerTests
{
    private readonly IBoardRepository _boardRepository = Substitute.For<IBoardRepository>();
    private readonly IActivityEntryRepository _activityRepository = Substitute.For<IActivityEntryRepository>();
    private readonly GetCardDetailHandler _handler;

    public GetCardDetailHandlerTests()
    {
        _handler = new GetCardDetailHandler(_boardRepository, _activityRepository);
    }

    [Fact]
    public async Task Handle_WithValidQuery_ShouldReturnCardDetailDto()
    {
        var projectId = Guid.NewGuid();
        var board = new BoardBuilder()
            .WithProjectId(projectId)
            .WithColumns("To Do")
            .WithCard("To Do", "Card 1", "Description")
            .WithLabel("Bug", LabelColors.Red)
            .WithCardLabel(0, 0)
            .Build();

        var cardId = board.Cards[0].Id;
        _boardRepository.GetBoardWithDetailsAsync(board.Id, Arg.Any<CancellationToken>())
            .Returns(board);
        _activityRepository.GetCardActivityAsync(
            board.Id, cardId, 11, null, Arg.Any<CancellationToken>())
            .Returns([]);

        var query = new GetCardDetailQuery(projectId, board.Id, cardId);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Id.Should().Be(cardId);
        result.Title.Should().Be("Card 1");
        result.Description.Should().Be("Description");
        result.Labels.Should().HaveCount(1);
        result.Labels[0].Name.Should().Be("Bug");
    }

    [Fact]
    public async Task Handle_WhenBoardNotFound_ShouldThrowNotFoundException()
    {
        var boardId = Guid.NewGuid();
        _boardRepository.GetBoardWithDetailsAsync(boardId, Arg.Any<CancellationToken>())
            .Returns((Board?)null);

        var query = new GetCardDetailQuery(Guid.NewGuid(), boardId, Guid.NewGuid());

        var act = () => _handler.Handle(query, CancellationToken.None);

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
        var query = new GetCardDetailQuery(differentProjectId, board.Id, board.Cards[0].Id);

        var act = () => _handler.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenCardNotFound_ShouldThrowNotFoundException()
    {
        var projectId = Guid.NewGuid();
        var board = new BoardBuilder()
            .WithProjectId(projectId)
            .WithColumns("To Do")
            .Build();

        _boardRepository.GetBoardWithDetailsAsync(board.Id, Arg.Any<CancellationToken>())
            .Returns(board);

        var nonExistentCardId = Guid.NewGuid();
        var query = new GetCardDetailQuery(projectId, board.Id, nonExistentCardId);

        var act = () => _handler.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WithMoreActivity_ShouldReturnNextCursor()
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

        var entries = Enumerable.Range(0, 11)
            .Select(_ => ActivityEntry.Create(
                board.Id, Guid.NewGuid(), "User",
                ActivityEntityType.Card, cardId,
                ActivityActionType.Created,
                new Dictionary<string, object>()))
            .ToList();

        _activityRepository.GetCardActivityAsync(
            board.Id, cardId, 11, null, Arg.Any<CancellationToken>())
            .Returns(entries);

        var query = new GetCardDetailQuery(projectId, board.Id, cardId);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Activity.Entries.Should().HaveCount(10);
        result.Activity.NextCursor.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_WithNoMoreActivity_ShouldReturnNullCursor()
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

        var entries = Enumerable.Range(0, 5)
            .Select(_ => ActivityEntry.Create(
                board.Id, Guid.NewGuid(), "User",
                ActivityEntityType.Card, cardId,
                ActivityActionType.Created,
                new Dictionary<string, object>()))
            .ToList();

        _activityRepository.GetCardActivityAsync(
            board.Id, cardId, 11, null, Arg.Any<CancellationToken>())
            .Returns(entries);

        var query = new GetCardDetailQuery(projectId, board.Id, cardId);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Activity.Entries.Should().HaveCount(5);
        result.Activity.NextCursor.Should().BeNull();
    }
}

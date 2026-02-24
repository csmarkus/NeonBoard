using NeonBoard.Application.Boards.Queries.GetArchivedCards;
using NeonBoard.Application.Common.Exceptions;
using NeonBoard.Application.Common.Interfaces;
using NeonBoard.Domain.Boards;
using NeonBoard.UnitTests.Builders;
using NSubstitute;

namespace NeonBoard.UnitTests.Application.Boards.Queries;

public class GetArchivedCardsHandlerTests
{
    private readonly IBoardRepository _boardRepository = Substitute.For<IBoardRepository>();
    private readonly GetArchivedCardsHandler _handler;

    public GetArchivedCardsHandlerTests()
    {
        _handler = new GetArchivedCardsHandler(_boardRepository);
    }

    [Fact]
    public async Task Handle_ShouldReturnOnlyArchivedCards()
    {
        var projectId = Guid.NewGuid();
        var board = new BoardBuilder()
            .WithProjectId(projectId)
            .WithName("Test Board")
            .WithColumns("To Do", "Done")
            .WithCard("To Do", "Active Card", "Active description")
            .WithCard("Done", "Archived Card", "Archived description")
            .Build();

        var archivedCardId = board.Cards.First(c => c.Content.Title == "Archived Card").Id;
        board.ArchiveCard(archivedCardId);
        board.ClearDomainEvents();

        _boardRepository.GetBoardWithDetailsAsync(board.Id, Arg.Any<CancellationToken>())
            .Returns(board);

        var query = new GetArchivedCardsQuery(projectId, board.Id);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().HaveCount(1);
        result[0].Title.Should().Be("Archived Card");
        result[0].Description.Should().Be("Archived description");
        result[0].ArchivedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_WhenNoArchivedCards_ShouldReturnEmptyList()
    {
        var projectId = Guid.NewGuid();
        var board = new BoardBuilder()
            .WithProjectId(projectId)
            .WithColumns("To Do")
            .WithCard("To Do", "Active Card", "Description")
            .Build();

        _boardRepository.GetBoardWithDetailsAsync(board.Id, Arg.Any<CancellationToken>())
            .Returns(board);

        var query = new GetArchivedCardsQuery(projectId, board.Id);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenBoardNotFound_ShouldThrowNotFoundException()
    {
        var boardId = Guid.NewGuid();
        _boardRepository.GetBoardWithDetailsAsync(boardId, Arg.Any<CancellationToken>())
            .Returns((Board?)null);

        var query = new GetArchivedCardsQuery(Guid.NewGuid(), boardId);

        var act = () => _handler.Handle(query, CancellationToken.None);

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
        var query = new GetArchivedCardsQuery(differentProjectId, board.Id);

        var act = () => _handler.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_ShouldOrderByArchivedAtDescending()
    {
        var projectId = Guid.NewGuid();
        var board = new BoardBuilder()
            .WithProjectId(projectId)
            .WithColumns("To Do")
            .WithCard("To Do", "First Archived", "Description 1")
            .WithCard("To Do", "Second Archived", "Description 2")
            .Build();

        var firstCardId = board.Cards.First(c => c.Content.Title == "First Archived").Id;
        board.ArchiveCard(firstCardId);

        // Small delay to ensure different ArchivedAt timestamps
        Thread.Sleep(50);

        var secondCardId = board.Cards.First(c => c.Content.Title == "Second Archived").Id;
        board.ArchiveCard(secondCardId);
        board.ClearDomainEvents();

        _boardRepository.GetBoardWithDetailsAsync(board.Id, Arg.Any<CancellationToken>())
            .Returns(board);

        var query = new GetArchivedCardsQuery(projectId, board.Id);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().HaveCount(2);
        // Most recently archived first
        result[0].Title.Should().Be("Second Archived");
        result[1].Title.Should().Be("First Archived");
    }
}

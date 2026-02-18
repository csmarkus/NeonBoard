using NeonBoard.Application.Boards.Queries.GetBoardDetails;
using NeonBoard.Application.Common.Exceptions;
using NeonBoard.Application.Common.Interfaces;
using NeonBoard.Domain.Boards;
using NeonBoard.Domain.Boards.ValueObjects;
using NeonBoard.UnitTests.Builders;
using NSubstitute;

namespace NeonBoard.UnitTests.Application.Boards.Queries;

public class GetBoardDetailsHandlerTests
{
    private readonly IBoardRepository _boardRepository = Substitute.For<IBoardRepository>();
    private readonly GetBoardDetailsHandler _handler;

    public GetBoardDetailsHandlerTests()
    {
        _handler = new GetBoardDetailsHandler(_boardRepository);
    }

    [Fact]
    public async Task Handle_WhenBoardExists_ShouldReturnBoardDetailsDto()
    {
        var projectId = Guid.NewGuid();
        var board = new BoardBuilder()
            .WithProjectId(projectId)
            .WithName("Test Board")
            .WithColumns("To Do", "In Progress", "Done")
            .WithCard("To Do", "Card 1", "Description 1")
            .WithCard("In Progress", "Card 2", "Description 2")
            .WithLabel("Bug", LabelColors.Red)
            .WithLabel("Feature", LabelColors.Blue)
            .WithCardLabel(0, 0)
            .Build();

        _boardRepository.GetBoardWithDetailsAsync(board.Id, Arg.Any<CancellationToken>())
            .Returns(board);

        var query = new GetBoardDetailsQuery(projectId, board.Id);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Id.Should().Be(board.Id);
        result.Name.Should().Be("Test Board");
        result.ProjectId.Should().Be(projectId);

        // Columns should be ordered by position
        result.Columns.Should().HaveCount(3);
        result.Columns[0].Name.Should().Be("To Do");
        result.Columns[1].Name.Should().Be("In Progress");
        result.Columns[2].Name.Should().Be("Done");
        result.Columns.Should().AllSatisfy(c => c.BoardId.Should().Be(board.Id));

        // Cards
        result.Cards.Should().HaveCount(2);
        var card1 = result.Cards.First(c => c.Title == "Card 1");
        card1.Description.Should().Be("Description 1");
        card1.Labels.Should().HaveCount(1);
        card1.Labels[0].Name.Should().Be("Bug");
        card1.Labels[0].Color.Should().Be(LabelColors.Red);

        var card2 = result.Cards.First(c => c.Title == "Card 2");
        card2.Labels.Should().BeEmpty();

        // Labels should be ordered by name
        result.Labels.Should().HaveCount(2);
        result.Labels[0].Name.Should().Be("Bug");
        result.Labels[1].Name.Should().Be("Feature");
    }

    [Fact]
    public async Task Handle_WhenBoardNotFound_ShouldThrowNotFoundException()
    {
        var boardId = Guid.NewGuid();
        _boardRepository.GetBoardWithDetailsAsync(boardId, Arg.Any<CancellationToken>())
            .Returns((Board?)null);

        var query = new GetBoardDetailsQuery(Guid.NewGuid(), boardId);

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
        var query = new GetBoardDetailsQuery(differentProjectId, board.Id);

        var act = () => _handler.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}

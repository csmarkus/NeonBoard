using NeonBoard.Application.Boards.Queries.GetBoardsByProject;
using NeonBoard.Application.Common.Interfaces;
using NeonBoard.Domain.Boards;
using NSubstitute;

namespace NeonBoard.UnitTests.Application.Boards.Queries;

public class GetBoardsByProjectHandlerTests
{
    private readonly IBoardRepository _boardRepository = Substitute.For<IBoardRepository>();
    private readonly GetBoardsByProjectHandler _handler;

    public GetBoardsByProjectHandlerTests()
    {
        _handler = new GetBoardsByProjectHandler(_boardRepository);
    }

    [Fact]
    public async Task Handle_WithBoards_ShouldReturnBoardDtoList()
    {
        var projectId = Guid.NewGuid();
        var board1 = Board.Create("Board 1", projectId);
        var board2 = Board.Create("Board 2", projectId);

        _boardRepository.GetBoardsByProjectIdAsync(projectId, Arg.Any<CancellationToken>())
            .Returns([board1, board2]);

        var query = new GetBoardsByProjectQuery(projectId);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().HaveCount(2);
        result[0].Name.Should().Be("Board 1");
        result[0].ProjectId.Should().Be(projectId);
        result[1].Name.Should().Be("Board 2");
    }

    [Fact]
    public async Task Handle_WithNoBoards_ShouldReturnEmptyList()
    {
        var projectId = Guid.NewGuid();
        _boardRepository.GetBoardsByProjectIdAsync(projectId, Arg.Any<CancellationToken>())
            .Returns([]);

        var query = new GetBoardsByProjectQuery(projectId);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().BeEmpty();
    }
}

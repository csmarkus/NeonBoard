using NeonBoard.Application.Boards.Commands.CreateBoard;
using NeonBoard.Application.Common.Interfaces;
using NeonBoard.Domain.Boards;
using NSubstitute;

namespace NeonBoard.UnitTests.Application.Boards.Commands;

public class CreateBoardHandlerTests
{
    private readonly IBoardRepository _boardRepository = Substitute.For<IBoardRepository>();
    private readonly CreateBoardHandler _handler;

    public CreateBoardHandlerTests()
    {
        _handler = new CreateBoardHandler(_boardRepository);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateBoardAndReturnDto()
    {
        var projectId = Guid.NewGuid();
        var command = new CreateBoardCommand(projectId, "Test Board");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Id.Should().NotBeEmpty();
        result.Name.Should().Be("Test Board");
        result.ProjectId.Should().Be(projectId);
        result.ColumnCount.Should().Be(0);
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        result.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task Handle_ShouldCallRepositoryAddAsync()
    {
        var projectId = Guid.NewGuid();
        var command = new CreateBoardCommand(projectId, "Test Board");

        await _handler.Handle(command, CancellationToken.None);

        await _boardRepository.Received(1).AddAsync(
            Arg.Is<Board>(b => b.Name == "Test Board" && b.ProjectId == projectId),
            Arg.Any<CancellationToken>());
    }
}

using NeonBoard.Application.Boards.Notifications;
using NeonBoard.Application.Common.Interfaces;
using NeonBoard.Domain.Boards;
using NeonBoard.Domain.Boards.Events;
using NSubstitute;

namespace NeonBoard.UnitTests.Application.Boards.Notifications;

public class BoardRealTimeNotificationHandlerTests
{
    private readonly IBoardNotificationService _notificationService = Substitute.For<IBoardNotificationService>();
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();
    private readonly IBoardRepository _boardRepository = Substitute.For<IBoardRepository>();
    private readonly BoardRealTimeNotificationHandler _handler;
    private readonly Guid _userId = Guid.NewGuid();

    public BoardRealTimeNotificationHandlerTests()
    {
        _currentUserService.GetUserIdAsync(Arg.Any<CancellationToken>()).Returns(_userId);
        _handler = new BoardRealTimeNotificationHandler(_notificationService, _currentUserService, _boardRepository);
    }

    [Fact]
    public async Task Handle_BoardCreatedEvent_ShouldSendProjectEvent()
    {
        var boardId = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        var evt = new BoardCreatedEvent(boardId, "Sprint Board", projectId, DateTime.UtcNow);

        await _handler.Handle(evt, CancellationToken.None);

        await _notificationService.Received(1)
            .SendProjectEventAsync(projectId, "BoardCreated", Arg.Any<object>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_BoardCreatedEvent_ShouldNotSendBoardEvent()
    {
        var boardId = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        var evt = new BoardCreatedEvent(boardId, "Sprint Board", projectId, DateTime.UtcNow);

        await _handler.Handle(evt, CancellationToken.None);

        await _notificationService.DidNotReceive()
            .SendBoardEventAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<object>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_BoardRenamedEvent_ShouldSendBoardEvent()
    {
        var projectId = Guid.NewGuid();
        var board = Board.Create("Test Board", projectId);
        _boardRepository.GetByIdAsync(board.Id, Arg.Any<CancellationToken>()).Returns(board);

        var evt = new BoardRenamedEvent(board.Id, "Old Name", "New Name");

        await _handler.Handle(evt, CancellationToken.None);

        await _notificationService.Received(1)
            .SendBoardEventAsync(board.Id, "BoardRenamed", Arg.Any<object>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_BoardRenamedEvent_ShouldSendProjectEvent_WhenBoardExists()
    {
        var projectId = Guid.NewGuid();
        var board = Board.Create("Test Board", projectId);
        _boardRepository.GetByIdAsync(board.Id, Arg.Any<CancellationToken>()).Returns(board);

        var evt = new BoardRenamedEvent(board.Id, "Old Name", "New Name");

        await _handler.Handle(evt, CancellationToken.None);

        await _notificationService.Received(1)
            .SendProjectEventAsync(projectId, "BoardRenamed", Arg.Any<object>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_BoardRenamedEvent_ShouldNotSendProjectEvent_WhenBoardNotFound()
    {
        var boardId = Guid.NewGuid();
        _boardRepository.GetByIdAsync(boardId, Arg.Any<CancellationToken>()).Returns((Board?)null);

        var evt = new BoardRenamedEvent(boardId, "Old Name", "New Name");

        await _handler.Handle(evt, CancellationToken.None);

        await _notificationService.Received(1)
            .SendBoardEventAsync(boardId, "BoardRenamed", Arg.Any<object>(), Arg.Any<CancellationToken>());
        await _notificationService.DidNotReceive()
            .SendProjectEventAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<object>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_BoardDeletedEvent_ShouldSendBothBoardAndProjectEvents()
    {
        var boardId = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        var evt = new BoardDeletedEvent(boardId, projectId, "Deleted Board");

        await _handler.Handle(evt, CancellationToken.None);

        await _notificationService.Received(1)
            .SendBoardEventAsync(boardId, "BoardDeleted", Arg.Any<object>(), Arg.Any<CancellationToken>());
        await _notificationService.Received(1)
            .SendProjectEventAsync(projectId, "BoardDeleted", Arg.Any<object>(), Arg.Any<CancellationToken>());
    }
}

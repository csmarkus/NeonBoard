using NeonBoard.Application.Boards.Notifications;
using NeonBoard.Application.Common.Interfaces;
using NeonBoard.Domain.Boards.Events;
using NSubstitute;

namespace NeonBoard.UnitTests.Application.Boards.Notifications;

public class LabelRealTimeNotificationHandlerTests
{
    private readonly IBoardNotificationService _notificationService = Substitute.For<IBoardNotificationService>();
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();
    private readonly LabelRealTimeNotificationHandler _handler;
    private readonly Guid _userId = Guid.NewGuid();

    public LabelRealTimeNotificationHandlerTests()
    {
        _currentUserService.GetUserIdAsync(Arg.Any<CancellationToken>()).Returns(_userId);
        _handler = new LabelRealTimeNotificationHandler(_notificationService, _currentUserService);
    }

    [Fact]
    public async Task Handle_LabelCreatedEvent_ShouldSendBoardEvent()
    {
        var boardId = Guid.NewGuid();
        var evt = new LabelCreatedEvent(boardId, Guid.NewGuid(), "Bug", "#ff0000");

        await _handler.Handle(evt, CancellationToken.None);

        await _notificationService.Received(1)
            .SendBoardEventAsync(boardId, "LabelCreated", Arg.Any<object>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_LabelUpdatedEvent_ShouldSendBoardEvent()
    {
        var boardId = Guid.NewGuid();
        var evt = new LabelUpdatedEvent(boardId, Guid.NewGuid(), "Feature", "#00ff00");

        await _handler.Handle(evt, CancellationToken.None);

        await _notificationService.Received(1)
            .SendBoardEventAsync(boardId, "LabelUpdated", Arg.Any<object>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_LabelRemovedEvent_ShouldSendBoardEvent()
    {
        var boardId = Guid.NewGuid();
        var evt = new LabelRemovedEvent(boardId, Guid.NewGuid(), "Old Label");

        await _handler.Handle(evt, CancellationToken.None);

        await _notificationService.Received(1)
            .SendBoardEventAsync(boardId, "LabelRemoved", Arg.Any<object>(), Arg.Any<CancellationToken>());
    }
}

namespace NeonBoard.Application.Common.Interfaces;

public interface IBoardNotificationService
{
    Task SendBoardEventAsync(Guid boardId, string eventType, object payload, CancellationToken cancellationToken = default);
    Task SendProjectEventAsync(Guid projectId, string eventType, object payload, CancellationToken cancellationToken = default);
}

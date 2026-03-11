using Microsoft.AspNetCore.SignalR;
using NeonBoard.Api.Hubs;
using NeonBoard.Application.Common.Interfaces;

namespace NeonBoard.Api.Services;

public class BoardNotificationService : IBoardNotificationService
{
    private readonly IHubContext<BoardHub> _hubContext;
    private readonly ILogger<BoardNotificationService> _logger;

    public BoardNotificationService(
        IHubContext<BoardHub> hubContext,
        ILogger<BoardNotificationService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task SendBoardEventAsync(
        Guid boardId, string eventType, object payload,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _hubContext.Clients
                .Group($"board:{boardId}")
                .SendAsync(eventType, payload, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send board event {EventType} to board {BoardId}", eventType, boardId);
        }
    }

    public async Task SendProjectEventAsync(
        Guid projectId, string eventType, object payload,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _hubContext.Clients
                .Group($"project:{projectId}")
                .SendAsync(eventType, payload, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send project event {EventType} to project {ProjectId}", eventType, projectId);
        }
    }
}

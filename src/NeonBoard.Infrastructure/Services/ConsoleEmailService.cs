using Microsoft.Extensions.Logging;
using NeonBoard.Application.Common.Interfaces;

namespace NeonBoard.Infrastructure.Services;

public class ConsoleEmailService : IEmailService
{
    private readonly ILogger<ConsoleEmailService> _logger;

    public ConsoleEmailService(ILogger<ConsoleEmailService> logger)
    {
        _logger = logger;
    }

    public Task SendInvitationEmailAsync(
        string email,
        string projectName,
        string inviterName,
        string token,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Invitation email for {Email} to join project '{ProjectName}' (invited by {InviterName}). " +
            "Accept at: /invite/{Token}",
            email, projectName, inviterName, token);

        return Task.CompletedTask;
    }
}

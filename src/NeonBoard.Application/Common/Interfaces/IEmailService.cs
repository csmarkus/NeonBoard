namespace NeonBoard.Application.Common.Interfaces;

public interface IEmailService
{
    Task SendInvitationEmailAsync(
        string email,
        string projectName,
        string inviterName,
        string token,
        CancellationToken cancellationToken = default);
}

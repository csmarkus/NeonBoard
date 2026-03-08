using MediatR;
using NeonBoard.Application.Common.Exceptions;
using NeonBoard.Application.Common.Interfaces;
using NeonBoard.Domain.Projects;

namespace NeonBoard.Application.Projects.Commands.RevokeInvitation;

public class RevokeInvitationHandler : IRequestHandler<RevokeInvitationCommand>
{
    private readonly IProjectInvitationRepository _invitationRepository;

    public RevokeInvitationHandler(IProjectInvitationRepository invitationRepository)
    {
        _invitationRepository = invitationRepository;
    }

    public async Task Handle(RevokeInvitationCommand request, CancellationToken cancellationToken)
    {
        var invitation = await _invitationRepository.GetByIdAsync(request.InvitationId, cancellationToken)
            ?? throw new NotFoundException(nameof(ProjectInvitation), request.InvitationId);

        if (invitation.ProjectId != request.ProjectId)
            throw new NotFoundException(nameof(ProjectInvitation), request.InvitationId);

        invitation.Revoke();
        await _invitationRepository.UpdateAsync(invitation, cancellationToken);
    }
}

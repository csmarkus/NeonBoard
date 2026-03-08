using MediatR;
using NeonBoard.Application.Common.Exceptions;
using NeonBoard.Application.Common.Interfaces;
using NeonBoard.Domain.Projects;

namespace NeonBoard.Application.Projects.Commands.AcceptInvitation;

public class AcceptInvitationHandler : IRequestHandler<AcceptInvitationCommand, Unit>
{
    private readonly IProjectInvitationRepository _invitationRepository;
    private readonly IProjectRepository _projectRepository;

    public AcceptInvitationHandler(
        IProjectInvitationRepository invitationRepository,
        IProjectRepository projectRepository)
    {
        _invitationRepository = invitationRepository;
        _projectRepository = projectRepository;
    }

    public async Task<Unit> Handle(AcceptInvitationCommand request, CancellationToken cancellationToken)
    {
        var invitation = await _invitationRepository.GetByTokenAsync(request.Token, cancellationToken)
            ?? throw new NotFoundException("Invitation", request.Token);

        invitation.Accept(request.UserId);

        var project = await _projectRepository.GetWithMembersAsync(invitation.ProjectId, cancellationToken)
            ?? throw new NotFoundException(nameof(Project), invitation.ProjectId);

        if (!project.IsMember(request.UserId))
        {
            project.AddMember(request.UserId, invitation.Role);
        }

        await _invitationRepository.UpdateAsync(invitation, cancellationToken);
        await _projectRepository.UpdateAsync(project, cancellationToken);

        return Unit.Value;
    }
}

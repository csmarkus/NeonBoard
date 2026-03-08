using MediatR;
using NeonBoard.Application.Common.Exceptions;
using NeonBoard.Application.Common.Interfaces;
using NeonBoard.Application.Projects.DTOs;
using NeonBoard.Domain.Projects;

namespace NeonBoard.Application.Projects.Queries.GetInvitationByToken;

public class GetInvitationByTokenHandler : IRequestHandler<GetInvitationByTokenQuery, InvitationDetailsDto>
{
    private readonly IProjectInvitationRepository _invitationRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly IUserRepository _userRepository;

    public GetInvitationByTokenHandler(
        IProjectInvitationRepository invitationRepository,
        IProjectRepository projectRepository,
        IUserRepository userRepository)
    {
        _invitationRepository = invitationRepository;
        _projectRepository = projectRepository;
        _userRepository = userRepository;
    }

    public async Task<InvitationDetailsDto> Handle(GetInvitationByTokenQuery request, CancellationToken cancellationToken)
    {
        var invitation = await _invitationRepository.GetByTokenAsync(request.Token, cancellationToken)
            ?? throw new NotFoundException("Invitation", request.Token);

        var project = await _projectRepository.GetByIdAsync(invitation.ProjectId, cancellationToken)
            ?? throw new NotFoundException(nameof(Project), invitation.ProjectId);

        var inviter = await _userRepository.GetByIdAsync(invitation.InvitedByUserId, cancellationToken);

        return new InvitationDetailsDto(
            invitation.Id,
            project.Name,
            inviter?.DisplayName ?? "A team member",
            invitation.Role,
            invitation.IsExpired() ? InvitationStatus.Expired : invitation.Status,
            invitation.IsExpired(),
            invitation.ExpiresAt);
    }
}

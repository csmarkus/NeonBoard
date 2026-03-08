using MediatR;
using NeonBoard.Application.Common.Interfaces;
using NeonBoard.Application.Projects.DTOs;
using NeonBoard.Domain.Projects;

namespace NeonBoard.Application.Projects.Queries.GetProjectInvitations;

public class GetProjectInvitationsHandler : IRequestHandler<GetProjectInvitationsQuery, List<ProjectInvitationDto>>
{
    private readonly IProjectInvitationRepository _invitationRepository;
    private readonly IUserRepository _userRepository;

    public GetProjectInvitationsHandler(
        IProjectInvitationRepository invitationRepository,
        IUserRepository userRepository)
    {
        _invitationRepository = invitationRepository;
        _userRepository = userRepository;
    }

    public async Task<List<ProjectInvitationDto>> Handle(GetProjectInvitationsQuery request, CancellationToken cancellationToken)
    {
        var invitations = await _invitationRepository.GetPendingByProjectIdAsync(request.ProjectId, cancellationToken);

        var inviterIds = invitations.Select(i => i.InvitedByUserId).Distinct().ToList();
        var inviters = await _userRepository.GetByIdsAsync(inviterIds, cancellationToken);
        var inviterLookup = inviters.ToDictionary(u => u.Id);

        return invitations.Select(i => new ProjectInvitationDto(
            i.Id,
            i.Email,
            i.Role,
            i.IsExpired() ? InvitationStatus.Expired : i.Status,
            i.ExpiresAt,
            inviterLookup.GetValueOrDefault(i.InvitedByUserId)?.DisplayName ?? "Unknown",
            i.CreatedAt))
        .OrderByDescending(i => i.CreatedAt)
        .ToList();
    }
}

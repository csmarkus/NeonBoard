using MediatR;
using NeonBoard.Application.Common.Exceptions;
using NeonBoard.Application.Common.Interfaces;
using NeonBoard.Application.Projects.DTOs;
using NeonBoard.Domain.Projects;

namespace NeonBoard.Application.Projects.Commands.InviteMember;

public class InviteMemberHandler : IRequestHandler<InviteMemberCommand, ProjectInvitationDto>
{
    private readonly IProjectRepository _projectRepository;
    private readonly IProjectInvitationRepository _invitationRepository;
    private readonly IUserRepository _userRepository;
    private readonly IEmailService _emailService;

    public InviteMemberHandler(
        IProjectRepository projectRepository,
        IProjectInvitationRepository invitationRepository,
        IUserRepository userRepository,
        IEmailService emailService)
    {
        _projectRepository = projectRepository;
        _invitationRepository = invitationRepository;
        _userRepository = userRepository;
        _emailService = emailService;
    }

    public async Task<ProjectInvitationDto> Handle(InviteMemberCommand request, CancellationToken cancellationToken)
    {
        var project = await _projectRepository.GetWithMembersAsync(request.ProjectId, cancellationToken)
            ?? throw new NotFoundException(nameof(Project), request.ProjectId);

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        // Check if user is already a member
        var existingUser = await _userRepository.GetByEmailAsync(normalizedEmail, cancellationToken);
        if (existingUser != null && project.IsMember(existingUser.Id))
            throw new ConflictException("User is already a member of this project.");

        // Check for pending invitation
        if (await _invitationRepository.HasPendingInvitationAsync(request.ProjectId, normalizedEmail, cancellationToken))
            throw new ConflictException("A pending invitation already exists for this email.");

        var invitation = ProjectInvitation.Create(
            request.ProjectId,
            normalizedEmail,
            request.Role,
            request.InvitedByUserId,
            DateTime.UtcNow.AddDays(7));

        await _invitationRepository.AddAsync(invitation, cancellationToken);

        var inviter = await _userRepository.GetByIdAsync(request.InvitedByUserId, cancellationToken);
        var inviterName = inviter?.DisplayName ?? "A team member";

        await _emailService.SendInvitationEmailAsync(
            normalizedEmail,
            project.Name,
            inviterName,
            invitation.Token,
            cancellationToken);

        return new ProjectInvitationDto(
            invitation.Id,
            invitation.Email,
            invitation.Role,
            invitation.Status,
            invitation.ExpiresAt,
            inviterName,
            invitation.CreatedAt);
    }
}

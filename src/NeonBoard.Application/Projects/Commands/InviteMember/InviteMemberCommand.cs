using MediatR;
using NeonBoard.Application.Projects.DTOs;
using NeonBoard.Domain.Projects;

namespace NeonBoard.Application.Projects.Commands.InviteMember;

public record InviteMemberCommand(
    Guid ProjectId,
    string Email,
    ProjectRole Role,
    Guid InvitedByUserId) : IRequest<ProjectInvitationDto>;

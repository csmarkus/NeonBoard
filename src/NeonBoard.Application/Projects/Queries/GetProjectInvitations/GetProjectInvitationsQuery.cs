using MediatR;
using NeonBoard.Application.Projects.DTOs;

namespace NeonBoard.Application.Projects.Queries.GetProjectInvitations;

public record GetProjectInvitationsQuery(Guid ProjectId) : IRequest<List<ProjectInvitationDto>>;

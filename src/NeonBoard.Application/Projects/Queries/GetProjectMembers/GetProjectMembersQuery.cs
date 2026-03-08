using MediatR;
using NeonBoard.Application.Projects.DTOs;

namespace NeonBoard.Application.Projects.Queries.GetProjectMembers;

public record GetProjectMembersQuery(Guid ProjectId) : IRequest<List<ProjectMemberDto>>;

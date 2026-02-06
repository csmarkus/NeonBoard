using MediatR;
using NeonBoard.Application.Projects.DTOs;

namespace NeonBoard.Application.Projects.Queries.GetProjectsByUser;

public record GetProjectsByUserQuery(Guid UserId) : IRequest<List<ProjectDto>>;

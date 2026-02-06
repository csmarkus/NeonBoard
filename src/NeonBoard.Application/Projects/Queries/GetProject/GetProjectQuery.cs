using MediatR;
using NeonBoard.Application.Projects.DTOs;

namespace NeonBoard.Application.Projects.Queries.GetProject;

public record GetProjectQuery(Guid ProjectId) : IRequest<ProjectDto>;

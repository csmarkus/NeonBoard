using NeonBoard.Domain.Projects;

namespace NeonBoard.Application.Projects.DTOs;

public record ProjectMemberDto(
    Guid UserId,
    string DisplayName,
    string Email,
    ProjectRole Role,
    DateTime JoinedAt);

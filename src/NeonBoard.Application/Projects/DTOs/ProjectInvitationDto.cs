using NeonBoard.Domain.Projects;

namespace NeonBoard.Application.Projects.DTOs;

public record ProjectInvitationDto(
    Guid Id,
    string Email,
    ProjectRole Role,
    InvitationStatus Status,
    DateTime ExpiresAt,
    string InvitedByName,
    DateTime CreatedAt);

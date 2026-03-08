using NeonBoard.Domain.Projects;

namespace NeonBoard.Application.Projects.DTOs;

public record InvitationDetailsDto(
    Guid Id,
    string ProjectName,
    string InviterName,
    ProjectRole Role,
    InvitationStatus Status,
    bool IsExpired,
    DateTime ExpiresAt);

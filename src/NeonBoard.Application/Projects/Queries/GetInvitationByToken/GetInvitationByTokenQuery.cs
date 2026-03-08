using MediatR;
using NeonBoard.Application.Projects.DTOs;

namespace NeonBoard.Application.Projects.Queries.GetInvitationByToken;

public record GetInvitationByTokenQuery(string Token) : IRequest<InvitationDetailsDto>;

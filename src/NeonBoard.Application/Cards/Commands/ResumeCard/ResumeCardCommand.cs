using MediatR;
using NeonBoard.Application.Cards.DTOs;

namespace NeonBoard.Application.Cards.Commands.ResumeCard;

public record ResumeCardCommand(Guid ProjectId, Guid BoardId, Guid CardId) : IRequest<CardDto>;

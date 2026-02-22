using MediatR;
using NeonBoard.Application.Cards.DTOs;

namespace NeonBoard.Application.Cards.Commands.RestoreCard;

public record RestoreCardCommand(Guid ProjectId, Guid BoardId, Guid CardId) : IRequest<CardDto>;

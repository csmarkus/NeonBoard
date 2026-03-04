using MediatR;
using NeonBoard.Application.Cards.DTOs;

namespace NeonBoard.Application.Cards.Commands.HoldCard;

public record HoldCardCommand(Guid ProjectId, Guid BoardId, Guid CardId) : IRequest<CardDto>;

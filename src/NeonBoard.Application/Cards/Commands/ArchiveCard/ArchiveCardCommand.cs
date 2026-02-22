using MediatR;
using NeonBoard.Application.Cards.DTOs;

namespace NeonBoard.Application.Cards.Commands.ArchiveCard;

public record ArchiveCardCommand(Guid ProjectId, Guid BoardId, Guid CardId) : IRequest<CardDto>;

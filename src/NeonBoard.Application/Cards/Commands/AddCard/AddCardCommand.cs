using MediatR;
using NeonBoard.Application.Cards.DTOs;

namespace NeonBoard.Application.Cards.Commands.AddCard;

public record AddCardCommand(
    Guid ProjectId,
    Guid BoardId,
    Guid ColumnId,
    string Title,
    string Description) : IRequest<CardDto>;

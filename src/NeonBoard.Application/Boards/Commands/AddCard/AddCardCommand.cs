using MediatR;
using NeonBoard.Application.Boards.DTOs;

namespace NeonBoard.Application.Boards.Commands.AddCard;

public record AddCardCommand(
    Guid ProjectId,
    Guid BoardId,
    Guid ColumnId,
    string Title,
    string Description) : IRequest<CardDto>;

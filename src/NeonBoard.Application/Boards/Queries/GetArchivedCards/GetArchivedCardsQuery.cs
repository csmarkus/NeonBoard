using MediatR;
using NeonBoard.Application.Cards.DTOs;

namespace NeonBoard.Application.Boards.Queries.GetArchivedCards;

public record GetArchivedCardsQuery(Guid ProjectId, Guid BoardId) : IRequest<List<CardDto>>;

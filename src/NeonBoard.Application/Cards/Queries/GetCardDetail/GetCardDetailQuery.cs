using MediatR;
using NeonBoard.Application.Cards.DTOs;

namespace NeonBoard.Application.Cards.Queries.GetCardDetail;

public record GetCardDetailQuery(
    Guid ProjectId,
    Guid BoardId,
    Guid CardId) : IRequest<CardDetailDto>;

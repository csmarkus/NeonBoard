using MediatR;
using NeonBoard.Application.Boards.DTOs;
using NeonBoard.Application.Cards.DTOs;
using NeonBoard.Application.Columns.DTOs;
using NeonBoard.Application.Common.Exceptions;
using NeonBoard.Application.Common.Interfaces;
using NeonBoard.Application.Labels.DTOs;
using NeonBoard.Domain.Boards;

namespace NeonBoard.Application.Boards.Queries.GetBoardDetails;

public class GetBoardDetailsHandler : IRequestHandler<GetBoardDetailsQuery, BoardDetailsDto>
{
    private readonly IBoardRepository _boardRepository;

    public GetBoardDetailsHandler(IBoardRepository boardRepository)
    {
        _boardRepository = boardRepository;
    }

    public async Task<BoardDetailsDto> Handle(GetBoardDetailsQuery request, CancellationToken cancellationToken)
    {
        var board = await _boardRepository.GetBoardWithDetailsAsync(request.BoardId, cancellationToken);
        if (board == null)
            throw new NotFoundException(nameof(Board), request.BoardId);

        if (board.ProjectId != request.ProjectId)
            throw new NotFoundException(nameof(Board), request.BoardId);

        var columns = board.Columns
            .OrderBy(c => c.Position.Value)
            .Select(c => new ColumnDto(
                c.Id,
                c.Name,
                c.Position.Value,
                board.Id))
            .ToList();

        var labels = board.Labels
            .OrderBy(l => l.Name)
            .Select(l => new LabelDto(l.Id, l.Name, l.Color))
            .ToList();

        var cards = board.Cards
            .Where(c => !c.IsArchived)
            .Select(c => CardDto.FromCard(c, board.Prefix.Value, labels))
            .ToList();

        return new BoardDetailsDto(
            board.Id,
            board.Name,
            board.Slug,
            board.Prefix.Value,
            board.ProjectId,
            board.CreatedAt,
            board.UpdatedAt,
            columns,
            cards,
            labels);
    }
}

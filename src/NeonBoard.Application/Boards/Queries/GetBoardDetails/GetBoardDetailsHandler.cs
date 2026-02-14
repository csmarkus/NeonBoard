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
            .Select(c => new CardDto(
                c.Id,
                c.Content.Title,
                c.Content.Description,
                c.ColumnId,
                c.Position.Value,
                c.LabelIds
                    .Select(labelId => labels.FirstOrDefault(l => l.Id == labelId))
                    .Where(label => label != null)
                    .Cast<LabelDto>()
                    .OrderBy(label => label.Name)
                    .ToList(),
                c.CreatedAt,
                c.UpdatedAt))
            .ToList();

        return new BoardDetailsDto(
            board.Id,
            board.Name,
            board.ProjectId,
            board.CreatedAt,
            board.UpdatedAt,
            columns,
            cards,
            labels);
    }
}

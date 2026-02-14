using MediatR;
using NeonBoard.Application.Common.Exceptions;
using NeonBoard.Application.Common.Interfaces;
using NeonBoard.Application.Labels.DTOs;
using NeonBoard.Domain.Boards;

namespace NeonBoard.Application.Labels.Commands.AddLabel;

public class AddLabelHandler : IRequestHandler<AddLabelCommand, LabelDto>
{
    private readonly IBoardRepository _boardRepository;

    public AddLabelHandler(IBoardRepository boardRepository)
    {
        _boardRepository = boardRepository;
    }

    public async Task<LabelDto> Handle(AddLabelCommand request, CancellationToken cancellationToken)
    {
        var board = await _boardRepository.GetBoardWithDetailsAsync(request.BoardId, cancellationToken);
        if (board == null)
            throw new NotFoundException(nameof(Board), request.BoardId);

        if (board.ProjectId != request.ProjectId)
            throw new NotFoundException(nameof(Board), request.BoardId);

        var labelId = board.AddLabel(request.Name, request.Color);

        var label = board.Labels.First(l => l.Id == labelId);
        return new LabelDto(label.Id, label.Name, label.Color);
    }
}

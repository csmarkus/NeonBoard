using MediatR;
using NeonBoard.Application.Common.Exceptions;
using NeonBoard.Application.Common.Interfaces;
using NeonBoard.Domain.Boards;

namespace NeonBoard.Application.Labels.Commands.UpdateLabel;

public class UpdateLabelHandler : IRequestHandler<UpdateLabelCommand, Unit>
{
    private readonly IBoardRepository _boardRepository;

    public UpdateLabelHandler(IBoardRepository boardRepository)
    {
        _boardRepository = boardRepository;
    }

    public async Task<Unit> Handle(UpdateLabelCommand request, CancellationToken cancellationToken)
    {
        var board = await _boardRepository.GetBoardWithDetailsAsync(request.BoardId, cancellationToken);
        if (board == null)
            throw new NotFoundException(nameof(Board), request.BoardId);

        if (board.ProjectId != request.ProjectId)
            throw new NotFoundException(nameof(Board), request.BoardId);

        board.UpdateLabel(request.LabelId, request.Name, request.Color);

        return Unit.Value;
    }
}

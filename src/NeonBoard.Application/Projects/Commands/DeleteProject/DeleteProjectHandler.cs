using MediatR;
using NeonBoard.Application.Common.Exceptions;
using NeonBoard.Application.Common.Interfaces;
using NeonBoard.Domain.Projects;

namespace NeonBoard.Application.Projects.Commands.DeleteProject;

public class DeleteProjectHandler : IRequestHandler<DeleteProjectCommand>
{
    private readonly IProjectRepository _projectRepository;
    private readonly IBoardRepository _boardRepository;

    public DeleteProjectHandler(
        IProjectRepository projectRepository,
        IBoardRepository boardRepository)
    {
        _projectRepository = projectRepository;
        _boardRepository = boardRepository;
    }

    public async Task Handle(DeleteProjectCommand request, CancellationToken cancellationToken)
    {
        var project = await _projectRepository.GetByIdAsync(request.ProjectId, cancellationToken);

        if (project == null)
            throw new NotFoundException(nameof(Project), request.ProjectId);

        // Delete all boards associated with the project
        var boards = await _boardRepository.GetBoardsByProjectIdAsync(request.ProjectId, cancellationToken);
        foreach (var board in boards)
        {
            await _boardRepository.DeleteAsync(board, cancellationToken);
        }

        await _projectRepository.DeleteAsync(project, cancellationToken);
    }
}

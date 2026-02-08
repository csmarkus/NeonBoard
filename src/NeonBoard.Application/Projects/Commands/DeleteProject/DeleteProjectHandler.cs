using MediatR;
using NeonBoard.Application.Common.Exceptions;
using NeonBoard.Application.Common.Interfaces;
using NeonBoard.Domain.Projects;

namespace NeonBoard.Application.Projects.Commands.DeleteProject;

public class DeleteProjectHandler : IRequestHandler<DeleteProjectCommand, Unit>
{
    private readonly IProjectRepository _projectRepository;

    public DeleteProjectHandler(IProjectRepository projectRepository)
    {
        _projectRepository = projectRepository;
    }

    public async Task<Unit> Handle(DeleteProjectCommand request, CancellationToken cancellationToken)
    {
        var project = await _projectRepository.GetByIdAsync(request.ProjectId, cancellationToken);

        if (project == null)
            throw new NotFoundException(nameof(Project), request.ProjectId);

        await _projectRepository.DeleteAsync(project, cancellationToken);

        return Unit.Value;
    }
}

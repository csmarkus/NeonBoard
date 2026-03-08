using MediatR;
using NeonBoard.Application.Common.Exceptions;
using NeonBoard.Application.Common.Interfaces;
using NeonBoard.Domain.Projects;

namespace NeonBoard.Application.Projects.Commands.RemoveMember;

public class RemoveMemberHandler : IRequestHandler<RemoveMemberCommand>
{
    private readonly IProjectRepository _projectRepository;

    public RemoveMemberHandler(IProjectRepository projectRepository)
    {
        _projectRepository = projectRepository;
    }

    public async Task Handle(RemoveMemberCommand request, CancellationToken cancellationToken)
    {
        var project = await _projectRepository.GetWithMembersAsync(request.ProjectId, cancellationToken)
            ?? throw new NotFoundException(nameof(Project), request.ProjectId);

        project.RemoveMember(request.UserId);
        await _projectRepository.UpdateAsync(project, cancellationToken);
    }
}

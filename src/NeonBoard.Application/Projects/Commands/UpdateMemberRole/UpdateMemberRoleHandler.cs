using MediatR;
using NeonBoard.Application.Common.Exceptions;
using NeonBoard.Application.Common.Interfaces;
using NeonBoard.Domain.Projects;

namespace NeonBoard.Application.Projects.Commands.UpdateMemberRole;

public class UpdateMemberRoleHandler : IRequestHandler<UpdateMemberRoleCommand>
{
    private readonly IProjectRepository _projectRepository;

    public UpdateMemberRoleHandler(IProjectRepository projectRepository)
    {
        _projectRepository = projectRepository;
    }

    public async Task Handle(UpdateMemberRoleCommand request, CancellationToken cancellationToken)
    {
        var project = await _projectRepository.GetWithMembersAsync(request.ProjectId, cancellationToken)
            ?? throw new NotFoundException(nameof(Project), request.ProjectId);

        project.UpdateMemberRole(request.UserId, request.NewRole);
        await _projectRepository.UpdateAsync(project, cancellationToken);
    }
}

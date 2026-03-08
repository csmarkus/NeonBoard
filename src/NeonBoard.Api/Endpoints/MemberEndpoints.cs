using MediatR;
using NeonBoard.Api.Filters;
using NeonBoard.Application.Common.Interfaces;
using NeonBoard.Application.Projects.Commands.InviteMember;
using NeonBoard.Application.Projects.Commands.LeaveProject;
using NeonBoard.Application.Projects.Commands.RemoveMember;
using NeonBoard.Application.Projects.Commands.UpdateMemberRole;
using NeonBoard.Application.Projects.Commands.RevokeInvitation;
using NeonBoard.Application.Projects.DTOs;
using NeonBoard.Application.Projects.Queries.GetProjectInvitations;
using NeonBoard.Application.Projects.Queries.GetProjectMembers;
using NeonBoard.Domain.Projects;

namespace NeonBoard.Api.Endpoints;

public static class MemberEndpoints
{
    public static void MapMemberEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/projects/{projectId:guid}/members")
            .WithTags("Members")
            .RequireAuthorization();

        group.MapGet("/", GetMembers)
            .AddEndpointFilter(ProjectAuth.Viewer())
            .Produces<List<ProjectMemberDto>>();

        group.MapPost("/invite", InviteMember)
            .AddEndpointFilter(ProjectAuth.Owner())
            .Produces<ProjectInvitationDto>(StatusCodes.Status201Created)
            .ProducesValidationProblem();

        group.MapDelete("/{userId:guid}", RemoveMember)
            .AddEndpointFilter(ProjectAuth.Owner())
            .Produces(StatusCodes.Status204NoContent);

        group.MapPut("/{userId:guid}/role", UpdateMemberRole)
            .AddEndpointFilter(ProjectAuth.Owner())
            .Produces(StatusCodes.Status204NoContent);

        group.MapPost("/leave", LeaveProject)
            .AddEndpointFilter(ProjectAuth.Viewer())
            .Produces(StatusCodes.Status204NoContent);

        // Invitations sub-group
        var invGroup = app.MapGroup("/api/projects/{projectId:guid}/invitations")
            .WithTags("Invitations")
            .RequireAuthorization();

        invGroup.MapGet("/", GetInvitations)
            .AddEndpointFilter(ProjectAuth.Owner())
            .Produces<List<ProjectInvitationDto>>();

        invGroup.MapDelete("/{invitationId:guid}", RevokeInvitation)
            .AddEndpointFilter(ProjectAuth.Owner())
            .Produces(StatusCodes.Status204NoContent);
    }

    private static async Task<IResult> GetMembers(
        Guid projectId, IMediator mediator, CancellationToken ct)
    {
        var result = await mediator.Send(new GetProjectMembersQuery(projectId), ct);
        return Results.Ok(result);
    }

    private static async Task<IResult> InviteMember(
        Guid projectId,
        InviteMemberRequest request,
        IMediator mediator,
        ICurrentUserService currentUserService,
        CancellationToken ct)
    {
        var userId = await currentUserService.GetUserIdAsync(ct);
        if (userId == null) return Results.Unauthorized();

        var command = new InviteMemberCommand(projectId, request.Email, request.Role, userId.Value);
        var result = await mediator.Send(command, ct);
        return Results.Created($"/api/projects/{projectId}/invitations/{result.Id}", result);
    }

    private static async Task<IResult> RemoveMember(
        Guid projectId, Guid userId, IMediator mediator, CancellationToken ct)
    {
        await mediator.Send(new RemoveMemberCommand(projectId, userId), ct);
        return Results.NoContent();
    }

    private static async Task<IResult> UpdateMemberRole(
        Guid projectId, Guid userId, UpdateMemberRoleRequest request,
        IMediator mediator, CancellationToken ct)
    {
        await mediator.Send(new UpdateMemberRoleCommand(projectId, userId, request.Role), ct);
        return Results.NoContent();
    }

    private static async Task<IResult> LeaveProject(
        Guid projectId, IMediator mediator,
        ICurrentUserService currentUserService, CancellationToken ct)
    {
        var userId = await currentUserService.GetUserIdAsync(ct);
        if (userId == null) return Results.Unauthorized();

        await mediator.Send(new LeaveProjectCommand(projectId, userId.Value), ct);
        return Results.NoContent();
    }

    private static async Task<IResult> GetInvitations(
        Guid projectId, IMediator mediator, CancellationToken ct)
    {
        var result = await mediator.Send(new GetProjectInvitationsQuery(projectId), ct);
        return Results.Ok(result);
    }

    private static async Task<IResult> RevokeInvitation(
        Guid projectId, Guid invitationId, IMediator mediator, CancellationToken ct)
    {
        await mediator.Send(new RevokeInvitationCommand(projectId, invitationId), ct);
        return Results.NoContent();
    }
}

public record InviteMemberRequest(string Email, ProjectRole Role);
public record UpdateMemberRoleRequest(ProjectRole Role);

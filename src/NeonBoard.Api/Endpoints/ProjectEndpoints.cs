using MediatR;
using NeonBoard.Api.Models;
using NeonBoard.Application.Projects.Commands.CreateProject;
using NeonBoard.Application.Projects.Commands.DeleteProject;
using NeonBoard.Application.Projects.Commands.UpdateProject;
using NeonBoard.Application.Projects.DTOs;
using NeonBoard.Application.Projects.Queries.GetProject;
using NeonBoard.Application.Projects.Queries.GetProjectsByUser;

namespace NeonBoard.Api.Endpoints;

public static class ProjectEndpoints
{
    public static void MapProjectEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/projects")
            .WithTags("Projects");

        group.MapPost("/", CreateProject)
            .WithName("CreateProject")
            .Produces<ProjectDto>(StatusCodes.Status201Created)
            .ProducesValidationProblem();

        group.MapGet("/{id:guid}", GetProject)
            .WithName("GetProject")
            .Produces<ProjectDto>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapGet("/user/{userId:guid}", GetProjectsByUser)
            .WithName("GetProjectsByUser")
            .Produces<List<ProjectDto>>();

        group.MapPut("/{id:guid}", UpdateProject)
            .WithName("UpdateProject")
            .Produces<ProjectDto>()
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:guid}", DeleteProject)
            .WithName("DeleteProject")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> CreateProject(
        CreateProjectRequest request,
        IMediator mediator,
        CancellationToken ct)
    {
        var command = new CreateProjectCommand(
            request.Name,
            request.Description,
            request.OwnerId);

        var result = await mediator.Send(command, ct);

        return Results.Created($"/api/projects/{result.Id}", result);
    }

    private static async Task<IResult> GetProject(
        Guid id,
        IMediator mediator,
        CancellationToken ct)
    {
        var query = new GetProjectQuery(id);
        var result = await mediator.Send(query, ct);
        return Results.Ok(result);
    }

    private static async Task<IResult> GetProjectsByUser(
        Guid userId,
        IMediator mediator,
        CancellationToken ct)
    {
        var query = new GetProjectsByUserQuery(userId);
        var result = await mediator.Send(query, ct);
        return Results.Ok(result);
    }

    private static async Task<IResult> UpdateProject(
        Guid id,
        UpdateProjectRequest request,
        IMediator mediator,
        CancellationToken ct)
    {
        var command = new UpdateProjectCommand(
            id,
            request.Name,
            request.Description);

        var result = await mediator.Send(command, ct);

        return Results.Ok(result);
    }

    private static async Task<IResult> DeleteProject(
        Guid id,
        IMediator mediator,
        CancellationToken ct)
    {
        var command = new DeleteProjectCommand(id);
        await mediator.Send(command, ct);
        return Results.NoContent();
    }
}

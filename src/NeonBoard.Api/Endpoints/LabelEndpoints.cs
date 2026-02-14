using MediatR;
using NeonBoard.Api.Filters;
using NeonBoard.Api.Models;
using NeonBoard.Application.Labels.Commands.AddLabel;
using NeonBoard.Application.Labels.Commands.UpdateLabel;
using NeonBoard.Application.Labels.Commands.RemoveLabel;
using NeonBoard.Application.Labels.DTOs;

namespace NeonBoard.Api.Endpoints;

public static class LabelEndpoints
{
    public static void MapLabelEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/projects/{projectId:guid}/boards/{boardId:guid}/labels")
            .WithTags("Labels")
            .RequireAuthorization()
            .AddEndpointFilter<ProjectOwnershipFilter>();

        group.MapPost("/", AddLabel)
            .WithName("AddLabel")
            .Produces<LabelDto>(StatusCodes.Status201Created)
            .ProducesValidationProblem();

        group.MapPut("/{labelId:guid}", UpdateLabel)
            .WithName("UpdateLabel")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem();

        group.MapDelete("/{labelId:guid}", RemoveLabel)
            .WithName("RemoveLabel")
            .Produces(StatusCodes.Status204NoContent);
    }

    private static async Task<IResult> AddLabel(
        Guid projectId,
        Guid boardId,
        AddLabelRequest request,
        IMediator mediator,
        CancellationToken ct)
    {
        var command = new AddLabelCommand(projectId, boardId, request.Name, request.Color);
        var result = await mediator.Send(command, ct);
        return Results.Created($"/api/projects/{projectId}/boards/{boardId}/labels/{result.Id}", result);
    }

    private static async Task<IResult> UpdateLabel(
        Guid projectId,
        Guid boardId,
        Guid labelId,
        UpdateLabelRequest request,
        IMediator mediator,
        CancellationToken ct)
    {
        var command = new UpdateLabelCommand(projectId, boardId, labelId, request.Name, request.Color);
        await mediator.Send(command, ct);
        return Results.NoContent();
    }

    private static async Task<IResult> RemoveLabel(
        Guid projectId,
        Guid boardId,
        Guid labelId,
        IMediator mediator,
        CancellationToken ct)
    {
        var command = new RemoveLabelCommand(projectId, boardId, labelId);
        await mediator.Send(command, ct);
        return Results.NoContent();
    }
}

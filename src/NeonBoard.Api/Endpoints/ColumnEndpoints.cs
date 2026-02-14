using MediatR;
using NeonBoard.Api.Filters;
using NeonBoard.Api.Models;
using NeonBoard.Application.Columns.Commands.AddColumn;
using NeonBoard.Application.Columns.Commands.RenameColumn;
using NeonBoard.Application.Columns.Commands.DeleteColumn;
using NeonBoard.Application.Columns.Commands.ReorderColumns;
using NeonBoard.Application.Columns.DTOs;

namespace NeonBoard.Api.Endpoints;

public static class ColumnEndpoints
{
    public static void MapColumnEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/projects/{projectId:guid}/boards/{boardId:guid}/columns")
            .WithTags("Columns")
            .RequireAuthorization()
            .AddEndpointFilter<ProjectOwnershipFilter>();

        group.MapPost("/", AddColumn)
            .WithName("AddColumn")
            .Produces<ColumnDto>(StatusCodes.Status201Created)
            .ProducesValidationProblem();

        group.MapPut("/{columnId:guid}", RenameColumn)
            .WithName("RenameColumn")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem();

        group.MapDelete("/{columnId:guid}", DeleteColumn)
            .WithName("DeleteColumn")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapPatch("/reorder", ReorderColumns)
            .WithName("ReorderColumns")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem();
    }

    private static async Task<IResult> AddColumn(
        Guid projectId,
        Guid boardId,
        AddColumnRequest request,
        IMediator mediator,
        CancellationToken ct)
    {
        var command = new AddColumnCommand(projectId, boardId, request.Name);
        var result = await mediator.Send(command, ct);
        return Results.Created($"/api/projects/{projectId}/boards/{boardId}/columns/{result.Id}", result);
    }

    private static async Task<IResult> RenameColumn(
        Guid projectId,
        Guid boardId,
        Guid columnId,
        RenameColumnRequest request,
        IMediator mediator,
        CancellationToken ct)
    {
        var command = new RenameColumnCommand(projectId, boardId, columnId, request.NewName);
        await mediator.Send(command, ct);
        return Results.NoContent();
    }

    private static async Task<IResult> DeleteColumn(
        Guid projectId,
        Guid boardId,
        Guid columnId,
        IMediator mediator,
        CancellationToken ct)
    {
        var command = new DeleteColumnCommand(projectId, boardId, columnId);
        await mediator.Send(command, ct);
        return Results.NoContent();
    }

    private static async Task<IResult> ReorderColumns(
        Guid projectId,
        Guid boardId,
        ReorderColumnsRequest request,
        IMediator mediator,
        CancellationToken ct)
    {
        var command = new ReorderColumnsCommand(projectId, boardId, request.ColumnIds);
        await mediator.Send(command, ct);
        return Results.NoContent();
    }
}

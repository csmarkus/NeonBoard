using MediatR;
using NeonBoard.Api.Models;
using NeonBoard.Application.Boards.Commands.CreateBoard;
using NeonBoard.Application.Boards.Commands.AddColumn;
using NeonBoard.Application.Boards.Commands.RenameColumn;
using NeonBoard.Application.Boards.Commands.DeleteColumn;
using NeonBoard.Application.Boards.Commands.ReorderColumns;
using NeonBoard.Application.Boards.Commands.AddCard;
using NeonBoard.Application.Boards.Commands.UpdateCard;
using NeonBoard.Application.Boards.Commands.MoveCard;
using NeonBoard.Application.Boards.Commands.DeleteCard;
using NeonBoard.Application.Boards.DTOs;
using NeonBoard.Application.Boards.Queries.GetBoardsByProject;
using NeonBoard.Application.Boards.Queries.GetBoardDetails;

namespace NeonBoard.Api.Endpoints;

public static class BoardEndpoints
{
    public static void MapBoardEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/projects/{projectId:guid}/boards")
            .WithTags("Boards")
            .RequireAuthorization();

        group.MapGet("/", GetBoardsByProject)
            .WithName("GetBoardsByProject")
            .Produces<List<BoardDto>>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapGet("/{boardId:guid}", GetBoardDetails)
            .WithName("GetBoardDetails")
            .Produces<BoardDetailsDto>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/", CreateBoard)
            .WithName("CreateBoard")
            .Produces<BoardDto>(StatusCodes.Status201Created)
            .ProducesValidationProblem();

        // Column endpoints
        group.MapPost("/{boardId:guid}/columns", AddColumn)
            .WithName("AddColumn")
            .Produces<ColumnDto>(StatusCodes.Status201Created)
            .ProducesValidationProblem();

        group.MapPut("/{boardId:guid}/columns/{columnId:guid}", RenameColumn)
            .WithName("RenameColumn")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem();

        group.MapDelete("/{boardId:guid}/columns/{columnId:guid}", DeleteColumn)
            .WithName("DeleteColumn")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapPatch("/{boardId:guid}/columns/reorder", ReorderColumns)
            .WithName("ReorderColumns")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem();

        // Card endpoints
        group.MapPost("/{boardId:guid}/cards", AddCard)
            .WithName("AddCard")
            .Produces<CardDto>(StatusCodes.Status201Created)
            .ProducesValidationProblem();

        group.MapPut("/{boardId:guid}/cards/{cardId:guid}", UpdateCard)
            .WithName("UpdateCard")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem();

        group.MapPatch("/{boardId:guid}/cards/{cardId:guid}/move", MoveCard)
            .WithName("MoveCard")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem();

        group.MapDelete("/{boardId:guid}/cards/{cardId:guid}", DeleteCard)
            .WithName("DeleteCard")
            .Produces(StatusCodes.Status204NoContent);
    }

    private static async Task<IResult> GetBoardsByProject(
        Guid projectId,
        IMediator mediator,
        CancellationToken ct)
    {
        var query = new GetBoardsByProjectQuery(projectId);
        var result = await mediator.Send(query, ct);
        return Results.Ok(result);
    }

    private static async Task<IResult> GetBoardDetails(
        Guid projectId,
        Guid boardId,
        IMediator mediator,
        CancellationToken ct)
    {
        var query = new GetBoardDetailsQuery(projectId, boardId);
        var result = await mediator.Send(query, ct);
        return Results.Ok(result);
    }

    private static async Task<IResult> CreateBoard(
        Guid projectId,
        CreateBoardRequest request,
        IMediator mediator,
        CancellationToken ct)
    {
        var command = new CreateBoardCommand(projectId, request.Name);
        var result = await mediator.Send(command, ct);
        return Results.Created($"/api/projects/{projectId}/boards/{result.Id}", result);
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

    private static async Task<IResult> AddCard(
        Guid projectId,
        Guid boardId,
        AddCardRequest request,
        IMediator mediator,
        CancellationToken ct)
    {
        var command = new AddCardCommand(
            projectId,
            boardId,
            request.ColumnId,
            request.Title,
            request.Description ?? string.Empty);
        var result = await mediator.Send(command, ct);
        return Results.Created($"/api/projects/{projectId}/boards/{boardId}/cards/{result.Id}", result);
    }

    private static async Task<IResult> UpdateCard(
        Guid projectId,
        Guid boardId,
        Guid cardId,
        UpdateCardRequest request,
        IMediator mediator,
        CancellationToken ct)
    {
        var command = new UpdateCardCommand(
            projectId,
            boardId,
            cardId,
            request.Title,
            request.Description ?? string.Empty);
        await mediator.Send(command, ct);
        return Results.NoContent();
    }

    private static async Task<IResult> MoveCard(
        Guid projectId,
        Guid boardId,
        Guid cardId,
        MoveCardRequest request,
        IMediator mediator,
        CancellationToken ct)
    {
        var command = new MoveCardCommand(
            projectId,
            boardId,
            cardId,
            request.TargetColumnId,
            request.TargetPosition);
        await mediator.Send(command, ct);
        return Results.NoContent();
    }

    private static async Task<IResult> DeleteCard(
        Guid projectId,
        Guid boardId,
        Guid cardId,
        IMediator mediator,
        CancellationToken ct)
    {
        var command = new DeleteCardCommand(projectId, boardId, cardId);
        await mediator.Send(command, ct);
        return Results.NoContent();
    }
}

using MediatR;
using NeonBoard.Api.Models;
using NeonBoard.Application.Boards.Commands.CreateBoard;
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
}

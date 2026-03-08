using MediatR;
using NeonBoard.Api.Filters;
using NeonBoard.Api.Models;
using NeonBoard.Application.Cards.Commands.AddCard;
using NeonBoard.Application.Cards.Commands.AddCardLabel;
using NeonBoard.Application.Cards.Commands.UpdateCard;
using NeonBoard.Application.Cards.Commands.MoveCard;
using NeonBoard.Application.Cards.Commands.DeleteCard;
using NeonBoard.Application.Cards.Commands.RemoveCardLabel;
using NeonBoard.Application.Cards.Commands.ArchiveCard;
using NeonBoard.Application.Cards.Commands.RestoreCard;
using NeonBoard.Application.Cards.Commands.HoldCard;
using NeonBoard.Application.Cards.Commands.ResumeCard;
using NeonBoard.Application.Cards.DTOs;
using NeonBoard.Application.Cards.Queries.GetCardDetail;
using NeonBoard.Application.Boards.Activity.DTOs;
using NeonBoard.Application.Boards.Activity.Queries.GetCardActivity;
using NeonBoard.Application.Boards.Queries.GetArchivedCards;

namespace NeonBoard.Api.Endpoints;

public static class CardEndpoints
{
    public static void MapCardEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/projects/{projectId:guid}/boards/{boardId:guid}/cards")
            .WithTags("Cards")
            .RequireAuthorization();

        group.MapPost("/", AddCard)
            .WithName("AddCard")
            .AddEndpointFilter(ProjectAuth.Editor())
            .Produces<CardDto>(StatusCodes.Status201Created)
            .ProducesValidationProblem();

        group.MapGet("/{cardId:guid}", GetCardDetail)
            .WithName("GetCardDetail")
            .AddEndpointFilter(ProjectAuth.Viewer())
            .Produces<CardDetailDto>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapGet("/archived", GetArchivedCards)
            .WithName("GetArchivedCards")
            .AddEndpointFilter(ProjectAuth.Viewer())
            .Produces<List<CardDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPut("/{cardId:guid}", UpdateCard)
            .WithName("UpdateCard")
            .AddEndpointFilter(ProjectAuth.Editor())
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem();

        group.MapPatch("/{cardId:guid}/move", MoveCard)
            .WithName("MoveCard")
            .AddEndpointFilter(ProjectAuth.Editor())
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem();

        group.MapDelete("/{cardId:guid}", DeleteCard)
            .WithName("DeleteCard")
            .AddEndpointFilter(ProjectAuth.Editor())
            .Produces(StatusCodes.Status204NoContent);

        group.MapPatch("/{cardId:guid}/archive", ArchiveCard)
            .WithName("ArchiveCard")
            .AddEndpointFilter(ProjectAuth.Editor())
            .Produces<CardDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPatch("/{cardId:guid}/restore", RestoreCard)
            .WithName("RestoreCard")
            .AddEndpointFilter(ProjectAuth.Editor())
            .Produces<CardDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPatch("/{cardId:guid}/hold", HoldCard)
            .WithName("HoldCard")
            .AddEndpointFilter(ProjectAuth.Editor())
            .Produces<CardDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPatch("/{cardId:guid}/resume", ResumeCard)
            .WithName("ResumeCard")
            .AddEndpointFilter(ProjectAuth.Editor())
            .Produces<CardDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPut("/{cardId:guid}/labels/{labelId:guid}", AddCardLabel)
            .WithName("AddCardLabel")
            .AddEndpointFilter(ProjectAuth.Editor())
            .Produces(StatusCodes.Status204NoContent);

        group.MapDelete("/{cardId:guid}/labels/{labelId:guid}", RemoveCardLabel)
            .WithName("RemoveCardLabel")
            .AddEndpointFilter(ProjectAuth.Editor())
            .Produces(StatusCodes.Status204NoContent);

        group.MapGet("/{cardId:guid}/activity", GetCardActivity)
            .WithName("GetCardActivity")
            .AddEndpointFilter(ProjectAuth.Viewer())
            .Produces<ActivityFeedDto>()
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> GetCardDetail(
        Guid projectId,
        Guid boardId,
        Guid cardId,
        IMediator mediator,
        CancellationToken ct)
    {
        var query = new GetCardDetailQuery(projectId, boardId, cardId);
        var result = await mediator.Send(query, ct);
        return Results.Ok(result);
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

    private static async Task<IResult> GetArchivedCards(
        Guid projectId,
        Guid boardId,
        IMediator mediator,
        CancellationToken ct)
    {
        var query = new GetArchivedCardsQuery(projectId, boardId);
        var result = await mediator.Send(query, ct);
        return Results.Ok(result);
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

    private static async Task<IResult> ArchiveCard(
        Guid projectId,
        Guid boardId,
        Guid cardId,
        IMediator mediator,
        CancellationToken ct)
    {
        var command = new ArchiveCardCommand(projectId, boardId, cardId);
        var result = await mediator.Send(command, ct);
        return Results.Ok(result);
    }

    private static async Task<IResult> RestoreCard(
        Guid projectId,
        Guid boardId,
        Guid cardId,
        IMediator mediator,
        CancellationToken ct)
    {
        var command = new RestoreCardCommand(projectId, boardId, cardId);
        var result = await mediator.Send(command, ct);
        return Results.Ok(result);
    }

    private static async Task<IResult> HoldCard(
        Guid projectId,
        Guid boardId,
        Guid cardId,
        IMediator mediator,
        CancellationToken ct)
    {
        var command = new HoldCardCommand(projectId, boardId, cardId);
        var result = await mediator.Send(command, ct);
        return Results.Ok(result);
    }

    private static async Task<IResult> ResumeCard(
        Guid projectId,
        Guid boardId,
        Guid cardId,
        IMediator mediator,
        CancellationToken ct)
    {
        var command = new ResumeCardCommand(projectId, boardId, cardId);
        var result = await mediator.Send(command, ct);
        return Results.Ok(result);
    }

    private static async Task<IResult> AddCardLabel(
        Guid projectId,
        Guid boardId,
        Guid cardId,
        Guid labelId,
        IMediator mediator,
        CancellationToken ct)
    {
        var command = new AddCardLabelCommand(projectId, boardId, cardId, labelId);
        await mediator.Send(command, ct);
        return Results.NoContent();
    }

    private static async Task<IResult> RemoveCardLabel(
        Guid projectId,
        Guid boardId,
        Guid cardId,
        Guid labelId,
        IMediator mediator,
        CancellationToken ct)
    {
        var command = new RemoveCardLabelCommand(projectId, boardId, cardId, labelId);
        await mediator.Send(command, ct);
        return Results.NoContent();
    }

    private static async Task<IResult> GetCardActivity(
        Guid projectId,
        Guid boardId,
        Guid cardId,
        int pageSize,
        DateTime? cursor,
        IMediator mediator,
        CancellationToken ct)
    {
        var query = new GetCardActivityQuery(projectId, boardId, cardId, pageSize, cursor);
        var result = await mediator.Send(query, ct);
        return Results.Ok(result);
    }
}

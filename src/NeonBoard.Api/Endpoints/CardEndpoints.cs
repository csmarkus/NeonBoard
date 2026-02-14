using MediatR;
using NeonBoard.Api.Filters;
using NeonBoard.Api.Models;
using NeonBoard.Application.Cards.Commands.AddCard;
using NeonBoard.Application.Cards.Commands.AddCardLabel;
using NeonBoard.Application.Cards.Commands.UpdateCard;
using NeonBoard.Application.Cards.Commands.MoveCard;
using NeonBoard.Application.Cards.Commands.DeleteCard;
using NeonBoard.Application.Cards.Commands.RemoveCardLabel;
using NeonBoard.Application.Cards.DTOs;

namespace NeonBoard.Api.Endpoints;

public static class CardEndpoints
{
    public static void MapCardEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/projects/{projectId:guid}/boards/{boardId:guid}/cards")
            .WithTags("Cards")
            .RequireAuthorization()
            .AddEndpointFilter<ProjectOwnershipFilter>();

        group.MapPost("/", AddCard)
            .WithName("AddCard")
            .Produces<CardDto>(StatusCodes.Status201Created)
            .ProducesValidationProblem();

        group.MapPut("/{cardId:guid}", UpdateCard)
            .WithName("UpdateCard")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem();

        group.MapPatch("/{cardId:guid}/move", MoveCard)
            .WithName("MoveCard")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem();

        group.MapDelete("/{cardId:guid}", DeleteCard)
            .WithName("DeleteCard")
            .Produces(StatusCodes.Status204NoContent);

        group.MapPut("/{cardId:guid}/labels/{labelId:guid}", AddCardLabel)
            .WithName("AddCardLabel")
            .Produces(StatusCodes.Status204NoContent);

        group.MapDelete("/{cardId:guid}/labels/{labelId:guid}", RemoveCardLabel)
            .WithName("RemoveCardLabel")
            .Produces(StatusCodes.Status204NoContent);
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
}

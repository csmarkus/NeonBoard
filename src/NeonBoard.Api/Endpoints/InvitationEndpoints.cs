using MediatR;
using NeonBoard.Application.Common.Interfaces;
using NeonBoard.Application.Projects.Commands.AcceptInvitation;
using NeonBoard.Application.Projects.DTOs;
using NeonBoard.Application.Projects.Queries.GetInvitationByToken;

namespace NeonBoard.Api.Endpoints;

public static class InvitationEndpoints
{
    public static void MapInvitationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/invitations")
            .WithTags("Invitations");

        group.MapGet("/{token}", GetInvitationByToken)
            .Produces<InvitationDetailsDto>();

        group.MapPost("/{token}/accept", AcceptInvitation)
            .RequireAuthorization()
            .Produces(StatusCodes.Status204NoContent);
    }

    private static async Task<IResult> GetInvitationByToken(
        string token, IMediator mediator, CancellationToken ct)
    {
        var result = await mediator.Send(new GetInvitationByTokenQuery(token), ct);
        return Results.Ok(result);
    }

    private static async Task<IResult> AcceptInvitation(
        string token, IMediator mediator,
        ICurrentUserService currentUserService, CancellationToken ct)
    {
        var userId = await currentUserService.GetUserIdAsync(ct);
        if (userId == null) return Results.Unauthorized();

        await mediator.Send(new AcceptInvitationCommand(token, userId.Value), ct);
        return Results.NoContent();
    }
}

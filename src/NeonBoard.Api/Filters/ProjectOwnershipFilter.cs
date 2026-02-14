using NeonBoard.Application.Common.Interfaces;

namespace NeonBoard.Api.Filters;

public class ProjectOwnershipFilter : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    {
        var projectId = GetProjectId(context.HttpContext);
        if (projectId == null)
            return Results.BadRequest("Project ID is required.");

        var currentUserService = context.HttpContext.RequestServices
            .GetRequiredService<ICurrentUserService>();
        var projectRepository = context.HttpContext.RequestServices
            .GetRequiredService<IProjectRepository>();

        var userId = await currentUserService.GetUserIdAsync(context.HttpContext.RequestAborted);
        if (userId == null)
            return Results.Unauthorized();

        var exists = await projectRepository.ExistsAsync(projectId.Value, context.HttpContext.RequestAborted);
        if (!exists)
            return Results.Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Not Found",
                detail: $"Project with ID {projectId.Value} was not found.");

        var isOwner = await projectRepository.IsProjectOwnedByUserAsync(
            projectId.Value, userId.Value, context.HttpContext.RequestAborted);

        if (!isOwner)
            return Results.Problem(
                statusCode: StatusCodes.Status403Forbidden,
                title: "Forbidden",
                detail: "You do not have permission to access this project.");

        return await next(context);
    }

    private static Guid? GetProjectId(HttpContext httpContext)
    {
        if (httpContext.Request.RouteValues.TryGetValue("projectId", out var projectIdValue)
            && Guid.TryParse(projectIdValue?.ToString(), out var projectId))
            return projectId;

        if (httpContext.Request.RouteValues.TryGetValue("id", out var idValue)
            && Guid.TryParse(idValue?.ToString(), out var id))
            return id;

        return null;
    }
}

using System.Net;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using NeonBoard.Application.Common.Exceptions;
using NeonBoard.Domain.Common;

namespace NeonBoard.Api.Middleware;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "An unhandled exception occurred: {Message}", exception.Message);

        var (statusCode, title, errors) = exception switch
        {
            Application.Common.Exceptions.NotFoundException notFoundEx => (
                StatusCodes.Status404NotFound,
                "Not Found",
                new Dictionary<string, string[]>
                {
                    ["error"] = [notFoundEx.Message]
                }),

            Application.Common.Exceptions.ValidationException validationEx => (
                StatusCodes.Status400BadRequest,
                "Validation Error",
                validationEx.Errors),

            FluentValidation.ValidationException fluentValidationEx => (
                StatusCodes.Status400BadRequest,
                "Validation Error",
                fluentValidationEx.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.ErrorMessage).ToArray())),

            DomainException domainEx => (
                StatusCodes.Status400BadRequest,
                "Domain Error",
                new Dictionary<string, string[]>
                {
                    ["error"] = [domainEx.Message]
                }),

            UnauthorizedAccessException => (
                StatusCodes.Status403Forbidden,
                "Forbidden",
                new Dictionary<string, string[]>
                {
                    ["error"] = ["You do not have permission to perform this action."]
                }),

            _ => (
                StatusCodes.Status500InternalServerError,
                "Internal Server Error",
                new Dictionary<string, string[]>
                {
                    ["error"] = ["An unexpected error occurred. Please try again later."]
                })
        };

        httpContext.Response.StatusCode = statusCode;

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Type = $"https://httpstatuses.com/{statusCode}",
            Extensions =
            {
                ["errors"] = errors
            }
        };

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}

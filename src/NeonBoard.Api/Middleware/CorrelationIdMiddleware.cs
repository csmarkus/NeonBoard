using Serilog.Context;

namespace NeonBoard.Api.Middleware;

public class CorrelationIdMiddleware
{
    private const string CORRELATION_ID_HEADER = "X-Correlation-Id";
    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers[CORRELATION_ID_HEADER].FirstOrDefault()
            ?? Guid.NewGuid().ToString();

        context.Response.Headers[CORRELATION_ID_HEADER] = correlationId;

        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            await _next(context);
        }
    }
}

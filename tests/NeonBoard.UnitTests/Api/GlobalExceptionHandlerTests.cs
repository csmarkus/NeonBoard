using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NeonBoard.Api.Middleware;
using NeonBoard.Application.Common.Exceptions;
using NeonBoard.Domain.Common;
using NSubstitute;

namespace NeonBoard.UnitTests.Api;

public class GlobalExceptionHandlerTests
{
    private readonly ILogger<GlobalExceptionHandler> _logger = Substitute.For<ILogger<GlobalExceptionHandler>>();
    private readonly GlobalExceptionHandler _handler;

    public GlobalExceptionHandlerTests()
    {
        _handler = new GlobalExceptionHandler(_logger);
    }

    [Fact]
    public async Task TryHandleAsync_WithNotFoundException_ShouldReturn404AndLogWarning()
    {
        var context = CreateHttpContext();
        var exception = new NotFoundException("Board", Guid.NewGuid());

        var result = await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        result.Should().BeTrue();
        context.Response.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        _logger.Received().Log(
            LogLevel.Warning,
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            exception,
            Arg.Any<Func<object, Exception?, string>>());
        _logger.DidNotReceive().Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            Arg.Any<Exception?>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task TryHandleAsync_WithValidationException_ShouldReturn400AndLogWarning()
    {
        var context = CreateHttpContext();
        var exception = new ValidationException();

        var result = await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        result.Should().BeTrue();
        context.Response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        _logger.Received().Log(
            LogLevel.Warning,
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            exception,
            Arg.Any<Func<object, Exception?, string>>());
        _logger.DidNotReceive().Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            Arg.Any<Exception?>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task TryHandleAsync_WithDomainException_ShouldReturn400AndLogWarning()
    {
        var context = CreateHttpContext();
        var exception = new DomainException("Business rule violated");

        var result = await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        result.Should().BeTrue();
        context.Response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        _logger.Received().Log(
            LogLevel.Warning,
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            exception,
            Arg.Any<Func<object, Exception?, string>>());
        _logger.DidNotReceive().Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            Arg.Any<Exception?>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task TryHandleAsync_WithUnhandledException_ShouldReturn500AndLogError()
    {
        var context = CreateHttpContext();
        var exception = new InvalidOperationException("Something went wrong");

        var result = await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        result.Should().BeTrue();
        context.Response.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        _logger.Received().Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            exception,
            Arg.Any<Func<object, Exception?, string>>());
        _logger.DidNotReceive().Log(
            LogLevel.Warning,
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            Arg.Any<Exception?>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    private static DefaultHttpContext CreateHttpContext()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        return context;
    }
}

using MediatR;
using Microsoft.Extensions.Logging;
using NeonBoard.Infrastructure.Persistence.Interceptors;
using NSubstitute;

namespace NeonBoard.UnitTests.Infrastructure;

public class DomainEventDispatcherInterceptorTests
{
    [Fact]
    public void Constructor_AcceptsLoggerDependency()
    {
        var mediator = Substitute.For<IMediator>();
        var logger = Substitute.For<ILogger<DomainEventDispatcherInterceptor>>();

        var interceptor = new DomainEventDispatcherInterceptor(mediator, logger);

        interceptor.Should().NotBeNull();
    }
}

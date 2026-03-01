using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using NeonBoard.Domain.Common;

namespace NeonBoard.Infrastructure.Persistence.Interceptors;

public class DomainEventDispatcherInterceptor : SaveChangesInterceptor
{
    private readonly IMediator _mediator;
    private readonly ILogger<DomainEventDispatcherInterceptor> _logger;

    public DomainEventDispatcherInterceptor(IMediator mediator, ILogger<DomainEventDispatcherInterceptor> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
        {
            await DispatchDomainEventsAsync(eventData.Context, cancellationToken);
        }

        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private async Task DispatchDomainEventsAsync(DbContext context, CancellationToken cancellationToken)
    {
        var entities = context.ChangeTracker
            .Entries<Entity>()
            .Where(e => e.Entity.GetDomainEvents().Any())
            .Select(e => e.Entity)
            .ToList();

        var domainEvents = entities
            .SelectMany(e => e.GetDomainEvents())
            .ToList();

        entities.ForEach(e => e.ClearDomainEvents());

        if (domainEvents.Count > 0)
        {
            _logger.LogDebug("Dispatching {EventCount} domain event(s)", domainEvents.Count);
        }

        foreach (var domainEvent in domainEvents)
        {
            _logger.LogDebug("Publishing domain event {EventType}", domainEvent.GetType().Name);
            await _mediator.Publish(domainEvent, cancellationToken);
        }
    }
}

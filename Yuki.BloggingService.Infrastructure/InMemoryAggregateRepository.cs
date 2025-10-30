using System.Collections.Concurrent;
using Yuki.BloggingService.Domain.Common;
using Yuki.BloggingService.Infrastructure.Messaging;

namespace Yuki.BloggingService.Infrastructure;

/// <summary>
/// This is a simple implementation of the IAggregateRepository.
/// This implementation is doing more things than it should (like publishing events).
/// In a production scenario, we would separate these concerns. But for simplicity and atomicity, we are combining them
/// here in one class to do both things.
///
/// In a real world scenario we would use
/// something like the outbox pattern to ensure that events are published only if the transaction
/// to save the aggregate was successful.
/// </summary>
public class InMemoryAggregateRepository(IEventBus eventBus) : IAggregateRepository
{
    private readonly ConcurrentDictionary<Guid, List<IEvent>> _eventStore = new();
    private readonly IEventBus _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));

    public async Task SaveAsync<T>(T aggregate, CancellationToken cancellationToken = default) where T: AggregateRoot, new()
    {
        var uncommitted = aggregate.GetUncommittedEvents().ToList();

        if (uncommitted.Count == 0)
            return;

        _eventStore.AddOrUpdate(
            aggregate.Id,
            _ => [..uncommitted],
            (_, existing) =>
            {
                lock (existing) // just in case, multiple threads are mutating the events...
                {
                    existing.AddRange(uncommitted);
                }
                return existing;
            });

        await _eventBus.PublishAsync(uncommitted, cancellationToken).ConfigureAwait(false);

        aggregate.ClearUncommittedEvents();
    }

    public Task<T> GetByIdAsync<T>(Guid id, CancellationToken cancellationToken = default) where T: AggregateRoot, new()
    {
        var aggregate = new T();
        if (_eventStore.TryGetValue(id, out var events))
        {
            aggregate.LoadFromHistory(events);
        }

        return Task.FromResult(aggregate);
    }
}

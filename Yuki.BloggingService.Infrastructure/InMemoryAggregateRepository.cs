using System.Collections.Concurrent;
using Yuki.BloggingService.Domain.Common;

namespace Yuki.BloggingService.Infrastructure;

/// <summary>
/// This is a simple implementation of the IAggregateRepository.
/// </summary>
public class InMemoryAggregateRepository : IAggregateRepository
{
    private readonly ConcurrentDictionary<Guid, List<IEvent>> _eventStore = new();
    
    // The interface is async, but for the test, since this is an in-memory collection of events, we 
    // will not use async/await as there is no I/O.
    public Task SaveAsync<T>(T aggregate) where T: AggregateRoot, new()
    {
        var uncommitted = aggregate.GetUncommittedEvents().ToList();

        if (uncommitted.Count == 0)
            return Task.CompletedTask;

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

        aggregate.ClearUncommittedEvents();
        return Task.CompletedTask;
    }

    public Task<T> GetByIdAsync<T>(Guid id) where T: AggregateRoot, new()
    {
        var aggregate = new T();
        if (_eventStore.TryGetValue(id, out var events))
        {
            aggregate.LoadFromHistory(events);
        }

        return Task.FromResult(aggregate);
    }
}
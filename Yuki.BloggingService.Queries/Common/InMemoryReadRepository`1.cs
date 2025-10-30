using System.Collections.Concurrent;
using System.Collections.ObjectModel;

namespace Yuki.Queries.Common;

public class InMemoryReadRepository<T> : IReadRepository<T>
{
    private readonly ConcurrentDictionary<Guid, T> _store = new();

    public Task Upsert(Guid id, T entity)
    {
        _store[id] = entity;
        return Task.CompletedTask;
    }

    public Task<bool> TryGetAsync(Guid id, out T entity, CancellationToken cancellationToken= default)
    {
        // ReSharper disable once NullableWarningSuppressionIsUsed
        return Task.FromResult(_store.TryGetValue(id, out entity!));
    }

    public Task<bool> TryUpdate(Guid id, Func<T, T> update)
    {
        ArgumentNullException.ThrowIfNull(update);

        while (_store.TryGetValue(id, out var current))
        {
            var updated = update(current);
            if (_store.TryUpdate(id, updated, current))
            {
                return Task.FromResult(true);
            }

            // Another thread updated the value before us; retry
        }

        return Task.FromResult(false);
    }

    public Task<ReadOnlyCollection<T>> GetAll()
    {
        return Task.FromResult(_store.Values.ToList().AsReadOnly());
    }
}

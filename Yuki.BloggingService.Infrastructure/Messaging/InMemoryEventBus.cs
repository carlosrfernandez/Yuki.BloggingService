using System.Collections.Concurrent;
using Yuki.BloggingService.Domain.Common;

namespace Yuki.BloggingService.Infrastructure.Messaging;

// DISCLAIMER:
// This is a simple event bus implementation for demonstration purposes.
// This should not be used in production systems.
// Additionally, this implementation does not account for delivery guarantees (at-least-once, etc.).
public sealed class InMemoryEventBus : IEventBus, IDisposable
{
    private readonly ConcurrentDictionary<Type, ConcurrentDictionary<Guid, Func<IEvent, Task>>> _handlers = new();
    private bool _disposed;

    public async Task PublishAsync(IEnumerable<IEvent> events, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(events);

        foreach (var @event in events)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var tasks = GetHandlersForEvent(@event)
                .Select(handler => handler(@event))
                .ToArray();

            if (tasks.Length == 0)
            {
                continue;
            }

            await Task.WhenAll(tasks).ConfigureAwait(false);
        }
    }

    public IDisposable Subscribe<TEvent>(Func<TEvent, Task> handler) where TEvent : class, IEvent
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(handler);

        var eventType = typeof(TEvent);
        var subscriptionId = Guid.NewGuid();

        var wrappedHandler = new Func<IEvent, Task>(evt => handler((TEvent)evt));

        var handlers = _handlers.GetOrAdd(eventType, _ => new ConcurrentDictionary<Guid, Func<IEvent, Task>>());
        handlers[subscriptionId] = wrappedHandler;

        return new Subscription(this, eventType, subscriptionId);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _handlers.Clear();
    }

    private IEnumerable<Func<IEvent, Task>> GetHandlersForEvent(IEvent @event)
    {
        var eventType = @event.GetType();
        foreach (var (subscriptionType, handlers) in _handlers)
        {
            if (subscriptionType.IsAssignableFrom(eventType))
            {
                foreach (var handler in handlers.Values)
                {
                    yield return handler;
                }
            }
        }
    }

    private void Unsubscribe(Type eventType, Guid subscriptionId)
    {
        if (_handlers.TryGetValue(eventType, out var handlers))
        {
            handlers.TryRemove(subscriptionId, out _);

            if (handlers.IsEmpty)
            {
                _handlers.TryRemove(eventType, out _);
            }
        }
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(InMemoryEventBus));
        }
    }

    private sealed class Subscription : IDisposable
    {
        private readonly InMemoryEventBus _eventBus;
        private readonly Type _eventType;
        private readonly Guid _subscriptionId;
        private int _disposed;

        public Subscription(InMemoryEventBus eventBus, Type eventType, Guid subscriptionId)
        {
            _eventBus = eventBus;
            _eventType = eventType;
            _subscriptionId = subscriptionId;
        }

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) != 0)
            {
                return;
            }

            _eventBus.Unsubscribe(_eventType, _subscriptionId);
        }
    }
}

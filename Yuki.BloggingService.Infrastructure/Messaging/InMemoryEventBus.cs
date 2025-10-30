using System.Reactive.Linq;
using System.Reactive.Subjects;
using Yuki.BloggingService.Domain.Common;

namespace Yuki.BloggingService.Infrastructure.Messaging;

// // DISCLAIMER:
// This is a simple event bus implementation for demonstration purposes
// This should not be used in production systems
// Additionally, this implementation does not take into consideration delivery policies (e.g., at-least-once, at-most-once, exactly-once)
// Only for demonstration and testing purposes!!! 
public sealed class InMemoryEventBus : IEventBus, IDisposable
{
    private readonly Subject<IEvent> _subject = new();
    private bool _disposed;

    public Task PublishAsync(IEnumerable<IEvent> events, CancellationToken cancellationToken = default)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(InMemoryEventBus));
        ArgumentNullException.ThrowIfNull(events);

        foreach (var @event in events)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _subject.OnNext(@event);
        }

        return Task.CompletedTask;
    }

    public IDisposable Subscribe<TEvent>(Action<TEvent> handler) where TEvent : class, IEvent
    {
        if (_disposed) throw new ObjectDisposedException(nameof(InMemoryEventBus));
        ArgumentNullException.ThrowIfNull(handler);
        return _subject.OfType<TEvent>().Subscribe(handler);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _subject.OnCompleted();
        _subject.Dispose();
    }
}

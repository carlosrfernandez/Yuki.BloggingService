using Yuki.BloggingService.Domain.Common;

namespace Yuki.BloggingService.Infrastructure.Messaging;

public interface IEventBus
{
    Task PublishAsync(IEnumerable<IEvent> events, CancellationToken cancellationToken = default);
    IDisposable Subscribe<TEvent>(Func<TEvent, Task> handler) where TEvent : class, IEvent;
}

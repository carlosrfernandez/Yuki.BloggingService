using NUnit.Framework;
using Yuki.BloggingService.Domain.Common;
using Yuki.BloggingService.Infrastructure.Messaging;

namespace Yuki.BloggingService.Infrastructure.Tests.Messaging;

[TestFixture]
public class InMemoryEventBusTests
{
    [Test]
    public async Task PublishAsync_ShouldDeliverEventsToSubscribers()
    {
        using var bus = new InMemoryEventBus();
        var received = new List<NameChanged>();
        using var _ = bus.Subscribe<NameChanged>(evt =>
        {
            received.Add(evt);
            return Task.CompletedTask;
        });

        var events = new[]
        {
            new NameChanged("Alice"),
            new NameChanged("Bob")
        };

        await bus.PublishAsync(events);

        Assert.That(received, Is.EquivalentTo(events));
    }

    [Test]
    public async Task PublishAsync_ShouldRespectSubscriptionDisposal()
    {
        using var bus = new InMemoryEventBus();
        var received = new List<NameChanged>();
        var subscription = bus.Subscribe<NameChanged>(evt =>
        {
            received.Add(evt);
            return Task.CompletedTask;
        });
        subscription.Dispose();

        await bus.PublishAsync([new NameChanged("Ignored")]);

        Assert.That(received, Is.Empty);
    }

    [Test]
    public void PublishAsync_ShouldThrowWhenCancelled()
    {
        using var bus = new InMemoryEventBus();
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await bus.PublishAsync([new NameChanged("Cancelled")], cts.Token));
    }

    [Test]
    public void PublishOrSubscribeAfterDispose_ShouldThrow()
    {
        var bus = new InMemoryEventBus();
        bus.Dispose();

        Assert.Multiple(() =>
        {
        Assert.ThrowsAsync<ObjectDisposedException>(async () => await bus.PublishAsync(Array.Empty<IEvent>()));

            Assert.That(
                () => bus.Subscribe<NameChanged>(_ => Task.CompletedTask),
                Throws.TypeOf<ObjectDisposedException>());
        });
    }
}

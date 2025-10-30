using NUnit.Framework;
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
        using var _ = bus.Subscribe<NameChanged>(received.Add);

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
        var subscription = bus.Subscribe<NameChanged>(received.Add);
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

        Assert.That(
            // ReSharper disable once AccessToDisposedClosure
            () => bus.PublishAsync([new NameChanged("Cancelled")], cts.Token),
            Throws.InstanceOf<OperationCanceledException>());
    }

    [Test]
    public void PublishOrSubscribeAfterDispose_ShouldThrow()
    {
        var bus = new InMemoryEventBus();
        bus.Dispose();

        Assert.Multiple(() =>
        {
            Assert.That(
                () => bus.PublishAsync([]),
                Throws.TypeOf<ObjectDisposedException>());

            Assert.That(
                () => bus.Subscribe<NameChanged>(_ => { }),
                Throws.TypeOf<ObjectDisposedException>());
        });
    }
}

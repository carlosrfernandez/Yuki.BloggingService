using NUnit.Framework;
using Yuki.BloggingService.Infrastructure.Messaging;

namespace Yuki.BloggingService.Infrastructure.Tests;

[TestFixture]
public class InMemoryAggregateRepositoryTests
{
    [Test]
    public async Task SaveAsync_Persists_Uncommitted_Events_And_Clears_Them()
    {
        using var eventBus = new InMemoryEventBus();
        var repo = new InMemoryAggregateRepository(eventBus);
        var aggregate = new TestAggregate();
        aggregate.ChangeName("Carlos");
        await repo.SaveAsync(aggregate);
        Assert.That(aggregate.GetUncommittedEvents(), Is.Empty, "uncommitted events must be cleared after save");

        var reloaded = await repo.GetByIdAsync<TestAggregate>(aggregate.Id);
        Assert.That(reloaded.Name, Is.EqualTo("Carlos"));
    }
    
    [Test]
    public async Task GetByIdAsync_Returns_Empty_Aggregate_When_Not_Found()
    {
        using var eventBus = new InMemoryEventBus();
        var repo = new InMemoryAggregateRepository(eventBus);
        var missingId = Guid.NewGuid();

        var aggregate = await repo.GetByIdAsync<TestAggregate>(missingId);
        Assert.That(aggregate, Is.Not.Null);
        Assert.That(aggregate.Id, Is.Not.EqualTo(Guid.Empty)); // ctor assigns new Guid
        Assert.That(aggregate.Name, Is.EqualTo(string.Empty));
    }
    
    [Test]
    public async Task SaveAsync_Appends_Events_For_Existing_Aggregate()
    {
        using var eventBus = new InMemoryEventBus();
        var repo = new InMemoryAggregateRepository(eventBus);
        var aggregate = new TestAggregate();
        aggregate.ChangeName("First");
        await repo.SaveAsync(aggregate);

        aggregate.ChangeName("Second");
        await repo.SaveAsync(aggregate);

        var reloaded = await repo.GetByIdAsync<TestAggregate>(aggregate.Id);
        Assert.That(reloaded.Name, Is.EqualTo("Second"));
        Assert.That(reloaded.Version, Is.EqualTo(1)); // two events were applied: First (0), Second (1)
    }
    
    [Test]
    public async Task SaveAsync_Does_Nothing_When_No_Uncommitted_Events()
    {
        using var eventBus = new InMemoryEventBus();
        var repo = new InMemoryAggregateRepository(eventBus);
        var aggregate = new TestAggregate();

        await repo.SaveAsync(aggregate);

        var reloaded = await repo.GetByIdAsync<TestAggregate>(aggregate.Id);
        // never saved anything, so history load should not change state
        Assert.That(reloaded.Name, Is.EqualTo(string.Empty));
        Assert.That(reloaded.Version, Is.EqualTo(-1)); // this is the default value
    }
    
    [Test]
    // [Repeat(1000000)]
    public async Task Concurrent_Saves_Do_Not_Lose_Events()
    {
        using var eventBus = new InMemoryEventBus();
        var repo = new InMemoryAggregateRepository(eventBus);
        var aggregate = new TestAggregate();

        // we simulate multiple changes, one after another, but each saved "concurrently"
        var names = new[] { "A", "B", "C", "D", "E" };

        var tasks = names.Select(async name =>
        {
            aggregate.ChangeName(name);
            await repo.SaveAsync(aggregate);
        });

        await Task.WhenAll(tasks);

        var reloaded = await repo.GetByIdAsync<TestAggregate>(aggregate.Id);
        // We check that we have processed all events. 
        // This is just testing the in-memory repository's thread-safety... not the aggregate itself.
        Assert.That(reloaded.Version, Is.EqualTo(names.Length - 1));
    }

    // This one is just testing that when an event is saved, it is also published to the event bus.
    [Test]
    public async Task SaveAsync_Publishes_Events_To_Bus()
    {
        using var eventBus = new InMemoryEventBus();
        var repo = new InMemoryAggregateRepository(eventBus);
        var aggregate = new TestAggregate();
        aggregate.ChangeName("Projected");

        string? observedName = null;
        using var subscription = eventBus.Subscribe<NameChanged>(evt => observedName = evt.Name);

        await repo.SaveAsync(aggregate);

        Assert.That(observedName, Is.EqualTo("Projected"));
    }
}

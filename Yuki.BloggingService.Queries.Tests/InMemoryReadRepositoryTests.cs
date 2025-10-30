using NUnit.Framework;
using Yuki.Queries.Common;

namespace Yuki.BloggingService.Queries.Tests;

[TestFixture]
public class InMemoryReadRepositoryTests
{
    private sealed record TestEntity(Guid Id, string Value);

    [Test]
    public async Task Upsert_ShouldStoreEntity()
    {
        var repository = new InMemoryReadRepository<TestEntity>();
        var id = Guid.NewGuid();
        var entity = new TestEntity(id, "value");

        await repository.Upsert(id, entity);

        var result = await repository.TryGetAsync(id, out var stored);
        Assert.That(result, Is.True);
        Assert.That(stored, Is.EqualTo(entity));
    }

    [Test]
    public async Task TryGet_WhenMissing_ShouldReturnFalse()
    {
        var repository = new InMemoryReadRepository<TestEntity>();
        var result = await repository.TryGetAsync(Guid.NewGuid(), out _);
        Assert.That(result, Is.False);
    }

    [Test]
    public async Task TryUpdate_WhenKeyExists_ShouldApplyTransformation()
    {
        var repository = new InMemoryReadRepository<TestEntity>();
        var id = Guid.NewGuid();
        await repository.Upsert(id, new TestEntity(id, "old"));

        var updated = await repository.TryUpdate(id, entity => entity with { Value = "new" });

        Assert.That(updated, Is.True);
        var result = await repository.TryGetAsync(id, out var stored);
        Assert.That(result, Is.True);
        Assert.That(stored.Value, Is.EqualTo("new"));
    }

    [Test]
    public async Task TryUpdate_WhenKeyMissing_ShouldReturnFalse()
    {
        var repository = new InMemoryReadRepository<TestEntity>();

        var result = await repository.TryUpdate(Guid.NewGuid(), entity => entity);

        Assert.That(result, Is.False);
    }

    [Test]
    public async Task List_ShouldReturnSnapshotOfValues()
    {
        var repository = new InMemoryReadRepository<TestEntity>();
        var first = new TestEntity(Guid.NewGuid(), "first");
        var second = new TestEntity(Guid.NewGuid(), "second");

        await repository.Upsert(first.Id, first);
        await repository.Upsert(second.Id, second);

        var snapshot = await repository.GetAll();

        Assert.That(snapshot, Has.Count.EqualTo(2));
        Assert.That(snapshot, Does.Contain(first));
        Assert.That(snapshot, Does.Contain(second));
    }

    [Test]
    public async Task TryUpdate_ShouldHandleConcurrentChanges()
    {
        var repository = new InMemoryReadRepository<TestEntity>();
        var id = Guid.NewGuid();
        await repository.Upsert(id, new TestEntity(id, "initial"));

        var task1 = Task.Run(async () => await repository.TryUpdate(id, entity => entity with { Value = "a" }));
        var task2 = Task.Run(async () => await repository.TryUpdate(id, entity => entity with { Value = "b" }));

        await Task.WhenAll(task1, task2);

        var result = await repository.TryGetAsync(id, out var stored);
        Assert.That(result, Is.True);
        Assert.That(stored.Value, Is.AnyOf("a", "b"));
    }
}

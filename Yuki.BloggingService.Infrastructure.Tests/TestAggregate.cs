using Yuki.BloggingService.Domain.Common;

namespace Yuki.BloggingService.Infrastructure.Tests;

public sealed class TestAggregate : AggregateRoot
{
    public string Name { get; private set; } = string.Empty;

    public TestAggregate()
    {
        // set an id for new aggregates
        Id = Guid.NewGuid();
    }

    public void ChangeName(string name)
    {
        ApplyChange(new NameChanged(name));
    }

    // this is what your base class's dynamic dispatch will call
    public void Apply(NameChanged e)
    {
        Name = e.Name;
    }
}
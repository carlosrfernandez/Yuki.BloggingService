namespace Yuki.Domain.Common;

public abstract class AggregateRoot
{
    private readonly List<IEvent> _uncommittedEvents = new();
    public IReadOnlyCollection<IEvent> GetUncommittedEvents() => _uncommittedEvents.AsReadOnly();
    public Guid Id { get; protected set; }
    public int Version { get; protected set; } = -1;
    
    public void ClearUncommittedEvents() => _uncommittedEvents.Clear();
    
    protected void ApplyChange(IEvent @event)
    {
        // Apply the event to the current aggregate state using dynamic dispatch
        // Note: This is for simplicity of the test. Dynamic dispatch does not have compile-time safety, and
        // its performance is worse than static dispatch. In production code, consider using a more robust approach.
        ((dynamic)this).Apply((dynamic)@event);

        // Add the event to uncommitted changes
        _uncommittedEvents.Add(@event);
    }

    public void LoadFromHistory(IEnumerable<IEvent> history)
    {
        foreach (var e in history)
        {
            // Same comment as above regarding dynamic dispatch
            ((dynamic)this).Apply((dynamic)e);
            Version++;
        }
    }
}
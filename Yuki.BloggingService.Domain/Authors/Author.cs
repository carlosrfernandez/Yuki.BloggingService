using Yuki.BloggingService.Domain.Common;

namespace Yuki.BloggingService.Domain.Authors;

public class Author : AggregateRoot
{
    public string Name { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public bool IsAuthorizedToPublish { get; private set; } = false;

    public IEnumerable<IEvent> RegisterAuthor(string name, string email)
    {
        if (Id != Guid.Empty) throw new InvalidOperationException("Only new authors can be registered.");
        Id = Guid.NewGuid();
        Name = name;
        Email = email;

        yield return new AuthorRegisteredEvent(Id, Name, Email, registeredAt: DateTimeOffset.UtcNow);
    }

    public IEnumerable<IEvent> AuthorizeToPublishPost()
    {
        if (Id != Guid.Empty) throw new InvalidOperationException("Only new authors can be registered.");
        if (IsAuthorizedToPublish)
        {
            // This is idempotent. if we get multiple authorize requests we ignore them.
            yield break;
        }
        
        IsAuthorizedToPublish = true;
        yield return new AuthorAuthorizedToPublishEvent(Id, authorizedAt: DateTimeOffset.UtcNow);
    }
    
    private void Apply(AuthorRegisteredEvent @event)
    {
        Id = @event.Id;
        Name = @event.Name;
        Email = @event.Email;
    }
    
    private void Apply(AuthorAuthorizedToPublishEvent _)
    {
        IsAuthorizedToPublish = true;
    }
}
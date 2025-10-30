using JetBrains.Annotations;
using Yuki.BloggingService.Domain.Common;

namespace Yuki.BloggingService.Domain.Authors;

public class Author : AggregateRoot
{
    public string Name { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public bool IsAuthorizedToPublish { get; private set; }

    public void Register(string name, string email)
    {
        if (Id != Guid.Empty) throw new InvalidOperationException("Only new authors can be registered.");

        RaiseEvent(new AuthorRegisteredEvent(
            Id,
            name,
            email,
            registeredAt: DateTimeOffset.UtcNow));
    }

    public void AuthorizeToPublishBlogPosts()
    {
        if (Id != Guid.Empty) throw new InvalidOperationException("Only new authors can be registered.");
        if (IsAuthorizedToPublish)
        {
            // This is idempotent. if we get multiple authorize requests we ignore them.
            return;
        }

        RaiseEvent(new AuthorAuthorizedToPublishEvent(Id, authorizedAt: DateTimeOffset.UtcNow));
    }

    [UsedImplicitly]
    private void Apply(AuthorRegisteredEvent @event)
    {
        Id = @event.Id;
        Name = @event.Name;
        Email = @event.Email;
    }

    [UsedImplicitly]
    private void Apply(AuthorAuthorizedToPublishEvent _)
    {
        IsAuthorizedToPublish = true;
    }
}
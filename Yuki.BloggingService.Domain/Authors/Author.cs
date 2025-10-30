using JetBrains.Annotations;
using Yuki.BloggingService.Domain.Common;
namespace Yuki.BloggingService.Domain.Authors;

public class Author : AggregateRoot
{
    public string Name { get; private set; } = string.Empty;
    public string Surname { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public bool IsAuthorizedToPublish { get; private set; }

    public void Register(string name, string surname, string email)
    {
        if (Id != Guid.Empty) throw new InvalidOperationException("Only new authors can be registered.");
        if (string.IsNullOrEmpty(name)) throw new InvalidOperationException("Author name cannot be empty.");
        if (string.IsNullOrEmpty(surname)) throw new InvalidOperationException("Author surname cannot be empty.");
        if (string.IsNullOrEmpty(email)) throw new InvalidOperationException("Author email cannot be empty.");
        
        RaiseEvent(new AuthorRegisteredEvent(
            Id,
            name,
            surname,
            email,
            registeredAt: DateTimeOffset.UtcNow));
    }

    public void AuthorizeToPublishBlogPosts()
    {
        if (Id == Guid.Empty) throw new InvalidOperationException("Unregistered authors cannot be authorized.");
        if (IsAuthorizedToPublish)
        {
            // This is idempotent. if we get multiple authorize requests we ignore them.
            return;
        }

        RaiseEvent(new AuthorAuthorizedToPublishEvent(Id, authorizedAt: DateTimeOffset.UtcNow));
    }

    [UsedImplicitly]
    public void Apply(AuthorRegisteredEvent @event)
    {
        Id = @event.Id;
        Name = @event.Name;
        Surname = @event.Surname;
        Email = @event.Email;
    }

    [UsedImplicitly]
    public void Apply(AuthorAuthorizedToPublishEvent _)
    {
        IsAuthorizedToPublish = true;
    }
}

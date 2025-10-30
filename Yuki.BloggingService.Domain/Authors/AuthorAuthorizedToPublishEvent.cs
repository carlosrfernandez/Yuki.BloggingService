using Yuki.BloggingService.Domain.Common;

namespace Yuki.BloggingService.Domain.Authors;

public class AuthorAuthorizedToPublishEvent(Guid id, DateTimeOffset authorizedAt) : IEvent
{
    public Guid Id { get; } = id;
    public DateTimeOffset AuthorizedAt { get; } = authorizedAt;
}
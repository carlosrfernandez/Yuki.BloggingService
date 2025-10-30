using Yuki.BloggingService.Domain.Common;

namespace Yuki.BloggingService.Domain.Authors;

public class AuthorRegisteredEvent(
    Guid id,
    string name,
    string email,
    DateTimeOffset registeredAt) : IEvent
{
    public Guid Id { get; } = id;
    public string Name { get; } = name;
    public string Email { get; } = email;
    public DateTimeOffset RegisteredAt { get; } = registeredAt;
}
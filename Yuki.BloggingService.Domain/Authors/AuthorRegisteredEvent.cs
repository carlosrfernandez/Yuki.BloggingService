using Yuki.BloggingService.Domain.Common;

namespace Yuki.BloggingService.Domain.Authors;

public class AuthorRegisteredEvent(
    Guid id,
    string name,
    string surname,
    string email,
    DateTimeOffset registeredAt) : IEvent
{
    public Guid Id { get; } = id;
    public string Name { get; } = name;
    public string Surname { get; } = surname;
    public string Email { get; } = email;
    public DateTimeOffset RegisteredAt { get; } = registeredAt;
}

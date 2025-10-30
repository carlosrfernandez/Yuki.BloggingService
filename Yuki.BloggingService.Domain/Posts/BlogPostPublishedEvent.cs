using Yuki.Domain.Common;

namespace Yuki.BloggingService.Domain.Posts;

public class BlogPostPublishedEvent(Guid id, DateTimeOffset publishedAt) : IEvent
{
    public Guid Id { get; } = id;
    public DateTimeOffset PublishedAt { get; } = publishedAt;
}
using Yuki.BloggingService.Domain.Common;

namespace Yuki.BloggingService.Domain.Posts;

public class BlogPostPublishedEvent(Guid id, Guid authorId, string authorName, DateTimeOffset publishedAt) : IEvent
{
    public Guid Id { get; set; } = id;
    public Guid AuthorId { get; set; } = authorId;
    public string AuthorName { get; set; } = authorName;
    public DateTimeOffset PublishedAt { get; } = publishedAt;
}
using Yuki.BloggingService.Domain.Common;

namespace Yuki.BloggingService.Domain.Posts;

public class BlogPostDraftCreatedEvent(
    Guid id,
    Guid authorId,
    string title,
    string description,
    string content,
    DateTimeOffset createdAt)
    : IEvent
{
    public Guid Id { get; } = id;
    public Guid AuthorId { get; } = authorId;
    public string Title { get; } = title;
    public string Description { get; } = description;
    public string Content { get; } = content;
    public DateTimeOffset CreatedAt { get; } = createdAt;
}
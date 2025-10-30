using Yuki.BloggingService.Domain.Common;

namespace Yuki.BloggingService.Domain.Posts;

public class BlogPost : AggregateRoot
{
    public Guid AuthorId { get; private set; } = Guid.Empty;
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string Content { get; private set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; private set; }
    public bool IsPublished { get; private set; } = false;
    
    /// <summary>
    /// Create a new blog post
    /// </summary>
    /// <param name="authorId"></param>
    /// <param name="title"></param>
    /// <param name="description"></param>
    /// <param name="content"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public IEnumerable<IEvent> DraftBlogPost(Guid authorId, string title, string description, string content)
    {
        if (Id == Guid.Empty) throw new InvalidOperationException("Only new blog posts can be created.");
        Id = Guid.NewGuid();
        AuthorId = authorId;
        Title = title;
        Description = description;
        Content = content;

        yield return new BlogPostDraftCreatedEvent(Id, AuthorId, Title, Description, Content,
            createdAt: DateTimeOffset.UtcNow);
    }
    
    public IEnumerable<IEvent> PublishBlogPost()
    {
        if (IsPublished) throw new InvalidOperationException("Blog post is already published.");

        yield return new BlogPostPublishedEvent(Id, publishedAt: DateTimeOffset.UtcNow);
    }

    /// <summary>
    /// To re-hydrate the state once we read from the event store
    /// </summary>
    /// <param name="event"></param>
    public void Apply(BlogPostDraftCreatedEvent @event)
    {
        Id = @event.Id;
        AuthorId = @event.AuthorId;
        Title = @event.Title;
        Description = @event.Description;
        Content = @event.Content;
        CreatedAt = @event.CreatedAt;
    }
    
    public void Apply(BlogPostPublishedEvent _)
    {
        IsPublished = true;
    }
}
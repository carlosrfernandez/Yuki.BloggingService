using Yuki.BloggingService.Domain.Authors;
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
    /// <param name="id"></param>
    /// <param name="authorId"></param>
    /// <param name="title"></param>
    /// <param name="description"></param>
    /// <param name="content"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public void DraftBlogPost(Guid id, Guid authorId, string title, string description, string content)
    {
        if (Id != Guid.Empty) throw new InvalidOperationException("Only new blog posts can be created.");
        if (authorId == Guid.Empty) throw new InvalidOperationException("Author cannot be empty.");
        if (string.IsNullOrWhiteSpace(title)) throw new InvalidOperationException("Title cannot be empty.");
        // Description is optional
        if (string.IsNullOrWhiteSpace(content)) throw new InvalidOperationException("Content cannot be empty.");

        RaiseEvent(new BlogPostDraftCreatedEvent(id, authorId, title, description, content, DateTimeOffset.UtcNow));
    }
    
    public void Publish(Author author)
    {
        if (IsPublished)
        {
            // No op - idempotent
            return;
        }
        
        if (!author.IsAuthorizedToPublish)
        {
            throw new InvalidOperationException("Author is not authorized to publish blog posts.");
        }
        
        RaiseEvent(new BlogPostPublishedEvent(Id, author.Id, author.Name, author.Surname, publishedAt: DateTimeOffset.UtcNow));
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

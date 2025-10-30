using System.Collections.Concurrent;
using System.Reactive.Disposables;
using Yuki.BloggingService.Domain.Posts;
using Yuki.BloggingService.Infrastructure.Messaging;
using Yuki.Queries.Common;

namespace Yuki.Queries.Projections;

// This has a simple storage. And should be treated as a persisted database or similar.
public sealed class BlogPostSummaryProjection(IEventBus eventBus) : ProjectionsBase
{
    private readonly IEventBus _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
    private readonly ConcurrentDictionary<Guid, BlogPostDraftSummary> _drafts = new();
    private CompositeDisposable? _subscriptions;

    public override void Start()
    {
        _subscriptions = new CompositeDisposable(2);
        _subscriptions.Add(_eventBus.Subscribe<BlogPostDraftCreatedEvent>(Handle));
        _subscriptions.Add(_eventBus.Subscribe<BlogPostPublishedEvent>(Handle));
    }

    public bool TryGetPost(Guid blogPostId, out BlogPostDraftSummary summary) =>
        _drafts.TryGetValue(blogPostId, out summary);

    private void Handle(BlogPostDraftCreatedEvent @event)
    {
        var summary = new BlogPostDraftSummary(
            @event.Id,
            @event.AuthorId,
            @event.Title,
            @event.Description,
            @event.Content,
            @event.CreatedAt, 
            null);

        _drafts[@event.Id] = summary;
    }

    private void Handle(BlogPostPublishedEvent @event)
    {
        var blogPost = _drafts.GetValueOrDefault(@event.Id);
        if (blogPost == null) return;

        var updatedRecord = blogPost with { PublishedAt = @event.PublishedAt };

        _drafts.TryUpdate(blogPost.Id, updatedRecord, blogPost);
    }

    public  override void Dispose()
    {
        _subscriptions?.Dispose();
    }
}

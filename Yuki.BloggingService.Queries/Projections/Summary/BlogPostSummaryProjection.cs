using System.Reactive.Disposables;
using Yuki.BloggingService.Domain.Posts;
using Yuki.BloggingService.Infrastructure.Messaging;
using Yuki.Queries.Common;

namespace Yuki.Queries.Projections.Summary;

// This has a simple storage. And should be treated as a persisted database or similar.
public sealed class BlogPostSummaryProjection(
    IEventBus eventBus,
    IReadRepository<BlogPostDraftSummary> repository) : ProjectionsBase
{
    private readonly IEventBus _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
    private readonly IReadRepository<BlogPostDraftSummary> _repository =
        repository ?? throw new ArgumentNullException(nameof(repository));
    private CompositeDisposable? _subscriptions;

    public override void Start()
    {
        _subscriptions = new CompositeDisposable(2);
        _subscriptions.Add(_eventBus.Subscribe<BlogPostDraftCreatedEvent>(Handle));
        _subscriptions.Add(_eventBus.Subscribe<BlogPostPublishedEvent>(Handle));
    }

    public Task<bool> TryGetDraft(Guid blogPostId, out BlogPostDraftSummary summary) =>
        _repository.TryGetAsync(blogPostId, out summary);

    private async Task Handle(BlogPostDraftCreatedEvent @event)
    {
        var summary = new BlogPostDraftSummary(
            @event.Id,
            @event.AuthorId,
            @event.Title,
            @event.Description,
            @event.Content,
            @event.CreatedAt, 
            null);

        await _repository.Upsert(@event.Id, summary);
    }

    private async Task Handle(BlogPostPublishedEvent @event)
    {
        await _repository.TryUpdate(
            @event.Id,
            existing => existing with { PublishedAt = @event.PublishedAt });
    }

    public  override void Dispose()
    {
        _subscriptions?.Dispose();
    }
}

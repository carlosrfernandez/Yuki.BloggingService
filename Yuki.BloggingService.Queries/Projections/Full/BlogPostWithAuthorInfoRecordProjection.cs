using System.Reactive.Disposables;
using Yuki.BloggingService.Domain.Posts;
using Yuki.BloggingService.Infrastructure.Messaging;
using Yuki.Queries.Common;

namespace Yuki.Queries.Projections.Full;

public sealed class BlogPostWithAuthorInfoRecordProjection(
    IEventBus eventBus,
    IReadRepository<BlogPostWithAuthorInformationRecord> repository) : ProjectionsBase
{
    private readonly IEventBus _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
    private readonly IReadRepository<BlogPostWithAuthorInformationRecord> _repository =
        repository ?? throw new ArgumentNullException(nameof(repository));
    private CompositeDisposable? _subscriptions;

    public override void Start()
    {
        _subscriptions = new CompositeDisposable(2);
        _subscriptions.Add(_eventBus.Subscribe<BlogPostDraftCreatedEvent>(Handle));
        _subscriptions.Add(_eventBus.Subscribe<BlogPostPublishedEvent>(Handle));
    }

    public override void Dispose()
    {
        _subscriptions?.Dispose();
    }

    private async Task Handle(BlogPostDraftCreatedEvent @event)
    {
        var record = new BlogPostWithAuthorInformationRecord
        {
            Id = @event.Id,
            AuthorId = @event.AuthorId,
            Title = @event.Title,
            Description = @event.Description,
            Content = @event.Content,
            CreatedAt = @event.CreatedAt
        };
        
        await _repository.Upsert(@event.Id, record);
    }

    private async Task Handle(BlogPostPublishedEvent @event)
    {
        await _repository.TryUpdate(@event.Id, record => record with
        {
            PublishedAt = @event.PublishedAt,
            AuthorId = @event.AuthorId,
            AuthorName = @event.AuthorName,
            AuthorSurname = @event.AuthorSurname
        });
    }
}

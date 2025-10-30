using System.Collections.Concurrent;
using System.Reactive.Disposables;
using Yuki.BloggingService.Domain.Posts;
using Yuki.BloggingService.Infrastructure.Messaging;
using Yuki.Queries.Common;

namespace Yuki.Queries.Projections;

public sealed class BlogPostWithAuthorInfoRecordProjection : ProjectionsBase
{
    private readonly ConcurrentDictionary<Guid, BlogPostWithAuthorInformationRecord> _records = new();
    private readonly CompositeDisposable _subscriptions;

    public BlogPostWithAuthorInfoRecordProjection(IEventBus eventBus)
    {
        _subscriptions = new CompositeDisposable(2);
        _subscriptions.Add(eventBus.Subscribe<BlogPostDraftCreatedEvent>(Handle));
        _subscriptions.Add(eventBus.Subscribe<BlogPostPublishedEvent>(Handle));
    }

    public override void Start()
    {
        throw new NotImplementedException();
    }


    public override void Dispose()
    {
        _subscriptions.Dispose();
    }

    private void Handle(BlogPostDraftCreatedEvent @event)
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
        
        _records[@event.Id] = record;
    }

    private void Handle(BlogPostPublishedEvent @event)
    {
        var record = _records.GetValueOrDefault(@event.Id);
        if (record is null)
        {
            return;
        }

        var updatedRecord = record with
        {
            PublishedAt = @event.PublishedAt,
            AuthorId = @event.AuthorId,
            AuthorName = @event.AuthorName,
            AuthorSurname = @event.AuthorSurname,
        };
        
        _records.TryUpdate(record.Id, updatedRecord, record);
    }
}
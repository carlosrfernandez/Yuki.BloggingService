using System;
using System.Threading.Tasks;
using NUnit.Framework;
using Yuki.BloggingService.Domain.Posts;
using Yuki.BloggingService.Infrastructure.Messaging;
using Yuki.Queries.Common;
using Yuki.Queries.Projections.Summary;

namespace Yuki.BloggingService.Queries.Tests;

[TestFixture]
public class BlogPostSummaryProjectionTests
{
    [Test]
    public async Task Start_WhenDraftEventArrives_ShouldStoreSummary()
    {
        using var eventBus = new InMemoryEventBus();
        var repository = new InMemoryReadRepository<BlogPostDraftSummary>();
        var projection = new BlogPostSummaryProjection(eventBus, repository);
        projection.Start();

        var blogPostId = Guid.NewGuid();
        var authorId = Guid.NewGuid();
        var createdAt = DateTimeOffset.UtcNow;
        var draftEvent = new BlogPostDraftCreatedEvent(
            blogPostId,
            authorId,
            "Title",
            "Description",
            "Content",
            createdAt);

        await eventBus.PublishAsync([draftEvent]);

        var result = await repository.TryGetAsync(blogPostId, out var summary);
        Assert.That(result, Is.True);

        Assert.Multiple(() =>
        {
            Assert.That(summary.Id, Is.EqualTo(blogPostId));
            Assert.That(summary.AuthorId, Is.EqualTo(authorId));
            Assert.That(summary.Title, Is.EqualTo("Title"));
            Assert.That(summary.Description, Is.EqualTo("Description"));
            Assert.That(summary.Content, Is.EqualTo("Content"));
            Assert.That(summary.CreatedAt, Is.EqualTo(createdAt));
            Assert.That(summary.PublishedAt, Is.Null);
        });

        projection.Dispose();
    }

    [Test]
    public async Task PublishEvent_ShouldUpdatePublishedAt()
    {
        using var eventBus = new InMemoryEventBus();
        var repository = new InMemoryReadRepository<BlogPostDraftSummary>();
        var projection = new BlogPostSummaryProjection(eventBus, repository);
        projection.Start();

        var blogPostId = Guid.NewGuid();
        var authorId = Guid.NewGuid();
        var createdAt = DateTimeOffset.UtcNow;
        var publishedAt = createdAt.AddHours(1);

        var draftEvent = new BlogPostDraftCreatedEvent(
            blogPostId,
            authorId,
            "Title",
            "Description",
            "Content",
            createdAt);

        await eventBus.PublishAsync([draftEvent]);

        var publishEvent = new BlogPostPublishedEvent(
            blogPostId,
            authorId,
            "Author",
            "Surname",
            publishedAt);

        await eventBus.PublishAsync([publishEvent]);
        var result = await repository.TryGetAsync(blogPostId, out var summary);
        Assert.That(result, Is.True);
        
        Assert.That(summary.PublishedAt, Is.EqualTo(publishedAt));

        projection.Dispose();
    }
}

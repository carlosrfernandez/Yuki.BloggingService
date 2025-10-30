using System;
using System.Threading.Tasks;
using NUnit.Framework;
using Yuki.BloggingService.Domain.Posts;
using Yuki.BloggingService.Infrastructure.Messaging;
using Yuki.Queries.Common;
using Yuki.Queries.Projections.Full;

namespace Yuki.BloggingService.Queries.Tests;

[TestFixture]
public class BlogPostWithAuthorInfoRecordProjectionTests
{
    [Test]
    public async Task DraftEvent_ShouldCreateRecord()
    {
        using var eventBus = new InMemoryEventBus();
        var repository = new InMemoryReadRepository<BlogPostWithAuthorInformationRecord>();
        var projection = new BlogPostWithAuthorInfoRecordProjection(eventBus, repository);
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
        var result = await repository.TryGetAsync(blogPostId, out var record);
        Assert.That(result, Is.True);
        
        Assert.Multiple(() =>
        {
            Assert.That(record.Id, Is.EqualTo(blogPostId));
            Assert.That(record.AuthorId, Is.EqualTo(authorId));
            Assert.That(record.Title, Is.EqualTo("Title"));
            Assert.That(record.Description, Is.EqualTo("Description"));
            Assert.That(record.Content, Is.EqualTo("Content"));
            Assert.That(record.CreatedAt, Is.EqualTo(createdAt));
            Assert.That(record.PublishedAt, Is.EqualTo(default(DateTimeOffset)));
            Assert.That(record.AuthorName, Is.EqualTo(string.Empty));
            Assert.That(record.AuthorSurname, Is.EqualTo(string.Empty));
        });

        projection.Dispose();
    }

    [Test]
    public async Task PublishEvent_ShouldEnrichRecordWithAuthorDetails()
    {
        using var eventBus = new InMemoryEventBus();
        var repository = new InMemoryReadRepository<BlogPostWithAuthorInformationRecord>();
        var projection = new BlogPostWithAuthorInfoRecordProjection(eventBus, repository);
        projection.Start();

        var blogPostId = Guid.NewGuid();
        var authorId = Guid.NewGuid();
        var createdAt = DateTimeOffset.UtcNow;
        var publishedAt = createdAt.AddMinutes(30);

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

        var result = await repository.TryGetAsync(blogPostId, out var record);
        Assert.That(result, Is.True);
        Assert.Multiple(() =>
        {
            Assert.That(record.PublishedAt, Is.EqualTo(publishedAt));
            Assert.That(record.AuthorName, Is.EqualTo("Author"));
            Assert.That(record.AuthorSurname, Is.EqualTo("Surname"));
            Assert.That(record.AuthorId, Is.EqualTo(authorId));
        });

        projection.Dispose();
    }
}

using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Threading.Tasks;
using NUnit.Framework;
using Yuki.BloggingService.Domain.Posts;
using Yuki.BloggingService.Infrastructure.Messaging;
using Yuki.Queries.Projections;

namespace Yuki.BloggingService.Queries.Tests;

[TestFixture]
public class BlogPostWithAuthorInfoRecordProjectionTests
{
    [Test]
    public async Task DraftEvent_ShouldCreateRecord()
    {
        using var eventBus = new InMemoryEventBus();
        var projection = new BlogPostWithAuthorInfoRecordProjection(eventBus);

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

        await eventBus.PublishAsync(new[] { draftEvent });

        var records = GetRecords(projection);
        Assert.That(records.TryGetValue(blogPostId, out var record), Is.True);

        Assert.Multiple(() =>
        {
            Assert.That(record!.Id, Is.EqualTo(blogPostId));
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
        var projection = new BlogPostWithAuthorInfoRecordProjection(eventBus);

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

        await eventBus.PublishAsync(new[] { draftEvent });

        var publishEvent = new BlogPostPublishedEvent(
            blogPostId,
            authorId,
            "Author",
            "Surname",
            publishedAt);

        await eventBus.PublishAsync(new[] { publishEvent });

        var records = GetRecords(projection);
        Assert.That(records.TryGetValue(blogPostId, out var record), Is.True);

        Assert.Multiple(() =>
        {
            Assert.That(record!.PublishedAt, Is.EqualTo(publishedAt));
            Assert.That(record.AuthorName, Is.EqualTo("Author"));
            Assert.That(record.AuthorSurname, Is.EqualTo("Surname"));
            Assert.That(record.AuthorId, Is.EqualTo(authorId));
        });

        projection.Dispose();
    }

    private static ConcurrentDictionary<Guid, BlogPostWithAuthorInformationRecord> GetRecords(
        BlogPostWithAuthorInfoRecordProjection projection)
    {
        var field = typeof(BlogPostWithAuthorInfoRecordProjection)
            .GetField("_records", BindingFlags.NonPublic | BindingFlags.Instance);

        return (ConcurrentDictionary<Guid, BlogPostWithAuthorInformationRecord>)field!.GetValue(projection)!;
    }
}

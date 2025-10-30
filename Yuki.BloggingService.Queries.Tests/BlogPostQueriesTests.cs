using System.Diagnostics;
using NUnit.Framework;
using Yuki.Queries;
using Yuki.Queries.Common;
using Yuki.Queries.Projections.Full;
using Yuki.Queries.Projections.Summary;

namespace Yuki.BloggingService.Queries.Tests;

[TestFixture]
public class BlogPostQueriesTests
{
    [Test]
    public async Task GetBlogPostInformation_ShouldReturnDraft_WhenExists()
    {
        var repository = new InMemoryReadRepository<BlogPostDraftSummaryRecord>();
        var publishedRepository = new InMemoryReadRepository<BlogPostWithAuthorInformationRecord>();
        var queries = new BlogPostQueries(repository, publishedRepository);

        var blogPostId = Guid.NewGuid();
        var draft = new BlogPostDraftSummaryRecord(
            blogPostId,
            Guid.NewGuid(),
            "Title",
            "Description",
            "Content",
            DateTimeOffset.UtcNow,
            null);

        await repository.Upsert(blogPostId, draft);

        var result = await queries.GetBlogPostInformation(blogPostId);

        Assert.That(result, Is.EqualTo(draft));
    }

    [Test]
    public async Task GetBlogPostInformationWithAuthorInfo_ShouldReturnRecord_WhenExists()
    {
        var repository = new InMemoryReadRepository<BlogPostDraftSummaryRecord>();
        var publishedRepository = new InMemoryReadRepository<BlogPostWithAuthorInformationRecord>();
        var queries = new BlogPostQueries(repository, publishedRepository);

        var blogPostId = Guid.NewGuid();
        var record = new BlogPostWithAuthorInformationRecord
        {
            Id = blogPostId,
            AuthorId = Guid.NewGuid(),
            Title = "Title",
            Description = "Description",
            Content = "Content",
            CreatedAt = DateTimeOffset.UtcNow,
            PublishedAt = DateTimeOffset.UtcNow.AddHours(1),
            AuthorName = "Author",
            AuthorSurname = "Surname"
        };

        await publishedRepository.Upsert(blogPostId, record);

        var result = await queries.GetBlogPostInformationWithAuthorInfo(blogPostId);

        Assert.That(result, Is.Not.Null);
        // To keep the compiler happy
        Debug.Assert(result != null, nameof(result) + " != null");
        Assert.That(result.Id, Is.EqualTo(blogPostId));
        Assert.That(result.AuthorName, Is.EqualTo("Author"));
        Assert.That(result.AuthorSurname, Is.EqualTo("Surname"));
    }

    [Test]
    public async Task Queries_ShouldReturnNull_WhenEntitiesAbsent()
    {
        var repository = new InMemoryReadRepository<BlogPostDraftSummaryRecord>();
        var publishedRepository = new InMemoryReadRepository<BlogPostWithAuthorInformationRecord>();
        var queries = new BlogPostQueries(repository, publishedRepository);

        var draft = await queries.GetBlogPostInformation(Guid.NewGuid());
        var published = await queries.GetBlogPostInformationWithAuthorInfo(Guid.NewGuid());

        Assert.That(draft, Is.Null);
        Assert.That(published, Is.Null);
    }
}
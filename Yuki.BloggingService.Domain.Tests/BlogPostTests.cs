using NUnit.Framework;
using Yuki.BloggingService.Domain.Authors;
using Yuki.BloggingService.Domain.Common;
using Yuki.BloggingService.Domain.Posts;

namespace Yuki.BloggingService.Domain.Tests;

[TestFixture]
public class BlogPostTests
{
    [Test]
    public void DraftBlogPost_ShouldSetState_AndRaiseEvent()
    {
        var blogPost = new BlogPost();
        var authorId = Guid.NewGuid();

        blogPost.DraftBlogPost(Guid.NewGuid(), authorId, "Title", "Description", "Content");

        Assert.That(blogPost.AuthorId, Is.EqualTo(authorId));
        Assert.That(blogPost.Title, Is.EqualTo("Title"));
        Assert.That(blogPost.Description, Is.EqualTo("Description"));
        Assert.That(blogPost.Content, Is.EqualTo("Content"));
        Assert.That(blogPost.CreatedAt, Is.Not.EqualTo(default(DateTimeOffset)));

        var events = blogPost.GetUncommittedEvents().ToList();
        Assert.That(events, Has.Count.EqualTo(1));
        Assert.That(events[0], Is.TypeOf<BlogPostDraftCreatedEvent>());

        var drafted = (BlogPostDraftCreatedEvent)events[0];
        Assert.That(drafted.AuthorId, Is.EqualTo(authorId));
        Assert.That(drafted.Title, Is.EqualTo("Title"));
        Assert.That(drafted.Description, Is.EqualTo("Description"));
        Assert.That(drafted.Content, Is.EqualTo("Content"));
    }
    
    [Test]
    public void DraftBlogPost_WithEmptyTitle_ShouldThrow()
    {
        var blogPost = new BlogPost();
        var authorId = Guid.NewGuid();

        Assert.That(() => blogPost.DraftBlogPost(Guid.NewGuid(), authorId, "", "Description", "Content"),
            Throws.InstanceOf<InvalidOperationException>());
    }

    [Test]
    public void DraftBlogPost_WithEmptyContent_ShouldThrow()
    {
        var blogPost = new BlogPost();
        var authorId = Guid.NewGuid();

        Assert.That(() => blogPost.DraftBlogPost(Guid.NewGuid(), authorId, "Title", "Description", ""),
            Throws.InstanceOf<InvalidOperationException>());
    }
    
    [Test]
    public void DraftBlogPost_WithEmptyAuthorId_ShouldThrow()
    {
        var blogPost = new BlogPost();

        Assert.That(() => blogPost.DraftBlogPost(Guid.NewGuid(), Guid.Empty, "Title", "Description", "Content"),
            Throws.InstanceOf<InvalidOperationException>());
    }
    
    [Test]
    public void DraftBlogPost_WhenAlreadyDrafted_ShouldThrow()
    {
        var blogPost = CreateDraftedBlogPost();

        Assert.That(() => blogPost.DraftBlogPost(Guid.NewGuid(),Guid.NewGuid(), "Other", "Other", "Other"),
            Throws.InstanceOf<InvalidOperationException>());
    }

    [Test]
    public void Publish_WithUnauthorizedAuthor_ShouldThrow()
    {
        var blogPost = CreateDraftedBlogPost();
        var author = CreateRegisteredAuthor();

        Assert.That(() => blogPost.Publish(author), Throws.InstanceOf<InvalidOperationException>());
    }

    [Test]
    public void Publish_ShouldRaiseEventAndSetFlag()
    {
        var blogPost = CreateDraftedBlogPost();
        var author = CreateAuthorizedAuthor();

        blogPost.Publish(author);

        Assert.That(blogPost.IsPublished, Is.True);

        var events = blogPost.GetUncommittedEvents().ToList();
        Assert.That(events, Has.Count.EqualTo(1));
        Assert.That(events[0], Is.TypeOf<BlogPostPublishedEvent>());

        var published = (BlogPostPublishedEvent)events[0];
        Assert.That(published.AuthorId, Is.EqualTo(author.Id));
        Assert.That(published.AuthorName, Is.EqualTo(author.Name));
        Assert.That(published.Id, Is.EqualTo(blogPost.Id));
        Assert.That(published.PublishedAt, Is.Not.EqualTo(default(DateTimeOffset)));
    }

    [Test]
    public void Publish_WhenAlreadyPublished_ShouldNotRaiseAdditionalEvents()
    {
        var blogPost = CreateDraftedBlogPost();
        var author = CreateAuthorizedAuthor();

        blogPost.Publish(author);
        blogPost.ClearUncommittedEvents();

        blogPost.Publish(author);

        Assert.That(blogPost.GetUncommittedEvents(), Is.Empty);
    }

    private static BlogPost CreateDraftedBlogPost()
    {
        var blogPost = new BlogPost();
        var drafted = new BlogPostDraftCreatedEvent(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Existing",
            "Existing description",
            "Existing content",
            DateTimeOffset.UtcNow);
        blogPost.LoadFromHistory([drafted]);
        return blogPost;
    }

    private static Author CreateRegisteredAuthor()
    {
        var author = new Author();
        var registered = new AuthorRegisteredEvent(Guid.NewGuid(), "Author", "author@example.com",
            DateTimeOffset.UtcNow);
        author.LoadFromHistory([registered]);
        return author;
    }

    private static Author CreateAuthorizedAuthor()
    {
        var author = CreateRegisteredAuthor();
        var authorized = new AuthorAuthorizedToPublishEvent(author.Id, DateTimeOffset.UtcNow);
        author.LoadFromHistory([authorized]);
        return author;
    }
}

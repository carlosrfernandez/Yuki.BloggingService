using Yuki.BloggingService.Domain.Authors;
using Yuki.BloggingService.Domain.Posts;

namespace Yuki.BloggingService.Application.Tests;

internal static class TestHelpers
{
    internal static Author BuildRegisteredAuthor(Guid authorId)
    {
        var author = new Author();
        var registered = new AuthorRegisteredEvent(authorId, "Alice", "alice@example.com", DateTimeOffset.UtcNow);
        author.LoadFromHistory([registered]);
        author.ClearUncommittedEvents();
        return author;
    }
    
    internal static Author BuildAuthorizedAuthor(this Author registeredAuthor)
    {
        var authorized = new AuthorAuthorizedToPublishEvent(registeredAuthor.Id, DateTimeOffset.UtcNow);
        registeredAuthor.LoadFromHistory([authorized]);
        registeredAuthor.ClearUncommittedEvents();
        return registeredAuthor;
    }

    internal static BlogPost BuildDraftedBlogPost(Guid blogPostId, Guid authorId)
    {
        var blogPost = new BlogPost();
        var drafted = new BlogPostDraftCreatedEvent(
            blogPostId,
            authorId,
            "Existing",
            "Existing description",
            "Existing content",
            DateTimeOffset.UtcNow);
        blogPost.LoadFromHistory([drafted]);
        blogPost.ClearUncommittedEvents();
        return blogPost;
    }
}
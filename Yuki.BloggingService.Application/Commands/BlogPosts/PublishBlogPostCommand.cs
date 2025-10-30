namespace Yuki.BloggingService.Application.Commands.BlogPosts;

public class PublishBlogPostCommand(Guid blogPostId, Guid authorId)
{
    public Guid BlogPostId { get; } = blogPostId;
    public Guid AuthorId { get; } = authorId;
}
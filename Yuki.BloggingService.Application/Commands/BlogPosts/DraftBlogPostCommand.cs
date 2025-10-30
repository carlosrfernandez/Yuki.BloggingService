namespace Yuki.BloggingService.Application.Commands.BlogPosts;

public sealed record DraftBlogPostCommand(
    Guid BlogPostId,
    Guid AuthorId,
    string Title,
    string Description,
    string Content);

using Yuki.BloggingService.Domain.Posts;

namespace Yuki.Application;

public interface IBlogPostAggregateRepository
{
    public Task<BlogPost> GetBlogPostByIdAsync(Guid blogPostId, CancellationToken cancellationToken = default);
}
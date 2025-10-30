using Yuki.Queries.Projections;

namespace Yuki.Queries.Common;

public interface IBlogPostQueries
{
    public Task<BlogPostDraftSummary?> GetBlogPostDraftByIdAsync(Guid blogPostId,
        CancellationToken cancellationToken = default);
    public Task<BlogPostWithAuthorInformationRecord?> GetPublishedBlogPostByIdAsync(Guid blogPostId,
        CancellationToken cancellationToken = default);
}
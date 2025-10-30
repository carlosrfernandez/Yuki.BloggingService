using Yuki.Queries.Common;
using Yuki.Queries.Projections;

namespace Yuki.Queries;

public class BlogPostQueries : IBlogPostQueries
{
    public Task<BlogPostDraftSummary?> GetBlogPostDraftByIdAsync(Guid blogPostId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<BlogPostWithAuthorInformationRecord?> GetPublishedBlogPostByIdAsync(Guid blogPostId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
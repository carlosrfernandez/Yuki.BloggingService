using Yuki.Queries.Projections.Full;
using Yuki.Queries.Projections.Summary;

namespace Yuki.Queries.Common;

public interface IBlogPostQueries
{
    Task<BlogPostDraftSummary?> GetBlogPostInformation(Guid blogPostId,
        CancellationToken cancellationToken = default);

    Task<BlogPostWithAuthorInformationRecord?> GetBlogPostInformationWithAuthorInfo(Guid blogPostId,
        CancellationToken cancellationToken = default);
}

using Yuki.Queries.Common;
using Yuki.Queries.Projections.Full;
using Yuki.Queries.Projections.Summary;

namespace Yuki.Queries;

/// <summary>
/// This class is, again, for simplicity. Ideally we have queries for each requirement.
/// We would have queries for each visualization that we need to provide...
/// Projections can be many, they can be specific, and the data is stored in an easy-to-query way.
/// This class is just to demonstrate how to implement queries using the read repositories.
/// </summary>
public class BlogPostQueries(
    IReadRepository<BlogPostDraftSummary> blogPostRepository,
    IReadRepository<BlogPostWithAuthorInformationRecord> blogPostWithAuthorRepository)
    : IBlogPostQueries
{
    public async Task<BlogPostDraftSummary?> GetBlogPostInformation(Guid blogPostId,
        CancellationToken cancellationToken = default)
    {
        var query = await blogPostRepository.TryGetAsync(blogPostId, out var blogPostSummary, cancellationToken);
        return !query ? null : blogPostSummary;
    }

    public async Task<BlogPostWithAuthorInformationRecord?> GetBlogPostInformationWithAuthorInfo(Guid blogPostId,
        CancellationToken cancellationToken = default)
    {
        var query = await blogPostWithAuthorRepository.TryGetAsync(blogPostId, out var blogPostSummary, cancellationToken);
        return !query ? null : blogPostSummary;
    }
}
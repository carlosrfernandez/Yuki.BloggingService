using Microsoft.AspNetCore.Mvc;
using Yuki.Queries.Common;
using Yuki.Queries.Projections.Full;
using Yuki.Queries.Projections.Summary;

namespace Yuki.BloggingService.Api.Controllers;

[ApiController]
[Route("api/blogposts")]
[Produces("application/json", "application/xml")]
public class BlogPostQueryController(IBlogPostQueries blogPostQueries) : ControllerBase
{
    private readonly IBlogPostQueries _blogPostQueries =
        blogPostQueries ?? throw new ArgumentNullException(nameof(blogPostQueries));

    [HttpGet("{blogPostId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBlogPost(
        Guid blogPostId,
        [FromQuery] bool includeAuthorInfo = false,
        CancellationToken cancellationToken = default)
    {
        var summary = await _blogPostQueries
            .GetBlogPostSummaryInformation(blogPostId, cancellationToken)
            .ConfigureAwait(false);

        if (summary is null)
            return NotFound();

        BlogPostWithAuthorInformationRecord? authorDetails = null;
        if (includeAuthorInfo)
        {
            authorDetails = await _blogPostQueries
                .GetBlogPostInformationWithAuthorInfo(blogPostId, cancellationToken)
                .ConfigureAwait(false);
        }

        return Ok(BlogPostResponse.From(summary, authorDetails));
    }

    private sealed record BlogPostResponse(
        Guid BlogPostId,
        Guid AuthorId,
        string Title,
        string Description,
        string Content,
        DateTimeOffset CreatedAt,
        bool IsPublished,
        DateTimeOffset? PublishedAt,
        string? AuthorName,
        string? AuthorSurname)
    {
        public static BlogPostResponse From(
            BlogPostDraftSummaryRecord summary,
            BlogPostWithAuthorInformationRecord? authorDetails)
        {
            var publishedAt = authorDetails?.PublishedAt ?? summary.PublishedAt;
            var isPublished = publishedAt is not null;

            return new BlogPostResponse(
                summary.Id,
                summary.AuthorId,
                summary.Title,
                summary.Description,
                summary.Content,
                summary.CreatedAt,
                isPublished,
                publishedAt,
                authorDetails?.AuthorName,
                authorDetails?.AuthorSurname);
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Yuki.BloggingService.Application.Commands.BlogPosts;
using Yuki.BloggingService.Infrastructure;

namespace Yuki.BloggingService.Api.Controllers;

[ApiController]
[Route("api/blogposts")]
[Produces("application/json")]
public class BlogPostCommandsController(
    ICommandHandler<DraftBlogPostCommand, Guid> draftHandler,
    ICommandHandler<PublishBlogPostCommand, bool> publishHandler) : ControllerBase
{
    private readonly ICommandHandler<DraftBlogPostCommand, Guid> _draftHandler =
        draftHandler ?? throw new ArgumentNullException(nameof(draftHandler));

    private readonly ICommandHandler<PublishBlogPostCommand, bool> _publishHandler =
        publishHandler ?? throw new ArgumentNullException(nameof(publishHandler));

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DraftBlogPost(
        [FromBody] DraftBlogPostRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var command = new DraftBlogPostCommand(
            request.AuthorId,
            request.Title,
            request.Description ?? string.Empty,
            request.Content);

        var blogpostId = await _draftHandler.HandleAsync(command, cancellationToken).ConfigureAwait(false);
        return Accepted(new { BlogpostId = blogpostId,message = "Blog post draft accepted." });
    }

    [HttpPost("{blogPostId:guid}/publish")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> PublishBlogPost(
        Guid blogPostId,
        [FromBody] PublishBlogPostRequest request,
        CancellationToken cancellationToken)
    {
        var command = new PublishBlogPostCommand(blogPostId, request.AuthorId);
        var result = await _publishHandler.HandleAsync(command, cancellationToken).ConfigureAwait(false);
        if (!result)
        {
            return UnprocessableEntity(new { message = "Blog post publishing failed." });
        }
        
        return Ok(new { message = "Blog post published successfully." });
    }

    public sealed record DraftBlogPostRequest(
        Guid AuthorId,
        string Title,
        string? Description,
        string Content);

    public sealed record PublishBlogPostRequest(Guid AuthorId);
}

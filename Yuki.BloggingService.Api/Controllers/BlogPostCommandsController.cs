using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Yuki.BloggingService.Application.Commands.BlogPosts;
using Yuki.BloggingService.Infrastructure;

namespace Yuki.BloggingService.Api.Controllers;

[ApiController]
[Route("api/blogposts")]
[Produces("application/json")]
public class BlogPostCommandsController(
    ICommandHandler<DraftBlogPostCommand> draftHandler,
    ICommandHandler<PublishBlogPostCommand> publishHandler) : ControllerBase
{
    private readonly ICommandHandler<DraftBlogPostCommand> _draftHandler =
        draftHandler ?? throw new ArgumentNullException(nameof(draftHandler));

    private readonly ICommandHandler<PublishBlogPostCommand> _publishHandler =
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

        await _draftHandler.HandleAsync(command, cancellationToken).ConfigureAwait(false);
        return Accepted(new { message = "Blog post draft accepted." });
    }

    [HttpPost("{blogPostId:guid}/publish")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> PublishBlogPost(
        Guid blogPostId,
        [FromBody] PublishBlogPostRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var command = new PublishBlogPostCommand(blogPostId, request.AuthorId);
        await _publishHandler.HandleAsync(command, cancellationToken).ConfigureAwait(false);
        return NoContent();
    }

    public sealed record DraftBlogPostRequest(
        [property: Required] Guid AuthorId,
        [property: Required] string Title,
        string? Description,
        [property: Required] string Content);

    public sealed record PublishBlogPostRequest([property: Required] Guid AuthorId);
}

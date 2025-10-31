using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Yuki.BloggingService.Application.Commands.Authors;
using Yuki.BloggingService.Infrastructure;

namespace Yuki.BloggingService.Api.Controllers;

[ApiController]
[Route("api/authors")]
[Produces("application/json", "application/xml")]
public class AuthorCommandsController(
    ICommandHandler<RegisterNewAuthorCommand> registerHandler,
    ICommandHandler<AuthorizeAuthorToPublishCommand> authorizeHandler) : ControllerBase
{
    private readonly ICommandHandler<RegisterNewAuthorCommand> _registerHandler =
        registerHandler ?? throw new ArgumentNullException(nameof(registerHandler));

    private readonly ICommandHandler<AuthorizeAuthorToPublishCommand> _authorizeHandler =
        authorizeHandler ?? throw new ArgumentNullException(nameof(authorizeHandler));

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RegisterAuthor(
        [FromBody] RegisterAuthorRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var command = new RegisterNewAuthorCommand
        {
            Name = request.Name,
            Surname = request.Surname,
            Email = request.Email
        };

        await _registerHandler.HandleAsync(command, cancellationToken).ConfigureAwait(false);
        return Accepted(new { message = "Author registration accepted." });
    }

    [HttpPost("{authorId:guid}/authorize")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> AuthorizeAuthor(Guid authorId, CancellationToken cancellationToken)
    {
        var command = new AuthorizeAuthorToPublishCommand { AuthorId = authorId };
        await _authorizeHandler.HandleAsync(command, cancellationToken).ConfigureAwait(false);
        return NoContent();
    }

    public sealed record RegisterAuthorRequest(
        [property: Required] string Name,
        [property: Required] string Surname,
        [property: Required, EmailAddress] string Email);
}

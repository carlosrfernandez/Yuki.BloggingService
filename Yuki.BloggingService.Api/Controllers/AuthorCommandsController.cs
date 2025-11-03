using Microsoft.AspNetCore.Mvc;
using Yuki.BloggingService.Application.Commands.Authors;
using Yuki.BloggingService.Infrastructure;

namespace Yuki.BloggingService.Api.Controllers;

[ApiController]
[Route("api/authors")]
[Produces("application/json", "application/xml")]
public class AuthorCommandsController(
    ICommandHandler<RegisterNewAuthorCommand, Guid> registerHandler,
    ICommandHandler<AuthorizeAuthorToPublishCommand, bool> authorizeHandler) : ControllerBase
{
    private readonly ICommandHandler<RegisterNewAuthorCommand, Guid> _registerHandler =
        registerHandler ?? throw new ArgumentNullException(nameof(registerHandler));

    private readonly ICommandHandler<AuthorizeAuthorToPublishCommand, bool> _authorizeHandler =
        authorizeHandler ?? throw new ArgumentNullException(nameof(authorizeHandler));

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RegisterAuthor(
        [FromBody] RegisterAuthorRequest request,
        CancellationToken cancellationToken)
    {
        var command = new RegisterNewAuthorCommand
        {
            Name = request.Name,
            Surname = request.Surname,
            Email = request.Email
        };

        var result = await _registerHandler.HandleAsync(command, cancellationToken).ConfigureAwait(false);
        return Accepted(new { AuthorId = result, message = "Author registration accepted." });
    }

    [HttpPost("{authorId:guid}/authorize")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> AuthorizeAuthor(Guid authorId, CancellationToken cancellationToken)
    {
        var command = new AuthorizeAuthorToPublishCommand { AuthorId = authorId };
        var result = await _authorizeHandler.HandleAsync(command, cancellationToken).ConfigureAwait(false);
        if (!result)
        {
            // again, this is just for the purposes of the exercise
            return UnprocessableEntity(new { message = "Author authorization failed." });
        }
        return NoContent();
    }

    public sealed record RegisterAuthorRequest
    {
        public RegisterAuthorRequest(string Name,
            string Surname,
            string Email)
        {
            this.Name = Name;
            this.Surname = Surname;
            this.Email = Email;
        }

        public string Name { get; set; }
        public string Surname { get; set; }
        public string Email { get; set; }
    }
}

using Yuki.BloggingService.Domain.Authors;
using Yuki.BloggingService.Domain.Common;
using Yuki.BloggingService.Infrastructure;

namespace Yuki.BloggingService.Application.Commands.Authors;

public class AuthorizeAuthorToPublishCommandHandler(IAggregateRepository authorRepository) 
    : ICommandHandler<AuthorizeAuthorToPublishCommand, bool>
{
    private readonly IAggregateRepository _authorRepository =
        authorRepository ?? throw new ArgumentNullException(nameof(authorRepository));

    public async Task<bool> HandleAsync(
        AuthorizeAuthorToPublishCommand command,
        CancellationToken cancellationToken = default)
    {
        var author = await _authorRepository.GetByIdAsync<Author>(command.AuthorId, cancellationToken);
        author.AuthorizeToPublishBlogPosts();
        await _authorRepository.SaveAsync(author, cancellationToken);
        return true;
    }
}
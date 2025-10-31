using Yuki.BloggingService.Domain.Common;
using Yuki.BloggingService.Domain.Posts;
using Yuki.BloggingService.Infrastructure;

namespace Yuki.BloggingService.Application.Commands.BlogPosts;

public sealed class DraftBlogPostCommandHandler(IAggregateRepository repository) : ICommandHandler<DraftBlogPostCommand, Guid>
{
    private readonly IAggregateRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));

    public async Task<Guid> HandleAsync(DraftBlogPostCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var id = Guid.NewGuid();
        var blogPost = await _repository.GetByIdAsync<BlogPost>(id, cancellationToken).ConfigureAwait(false);
        blogPost.DraftBlogPost(id, command.AuthorId, command.Title, command.Description, command.Content);
        
        await _repository.SaveAsync(blogPost, cancellationToken).ConfigureAwait(false);
        return blogPost.Id;
    }
}

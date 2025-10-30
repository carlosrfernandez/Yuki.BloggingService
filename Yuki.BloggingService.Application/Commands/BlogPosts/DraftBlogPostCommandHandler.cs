using Yuki.BloggingService.Domain.Common;
using Yuki.BloggingService.Domain.Posts;
using Yuki.BloggingService.Infrastructure;

namespace Yuki.BloggingService.Application.Commands.BlogPosts;

public sealed class DraftBlogPostCommandHandler(IAggregateRepository repository) : ICommandHandler<DraftBlogPostCommand>
{
    private readonly IAggregateRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));

    public async Task HandleAsync(DraftBlogPostCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var blogPost = await _repository.GetByIdAsync<BlogPost>(command.BlogPostId, cancellationToken).ConfigureAwait(false);
        blogPost.DraftBlogPost(command.AuthorId, command.Title, command.Description, command.Content);
        
        await _repository.SaveAsync(blogPost, cancellationToken).ConfigureAwait(false);
    }
}

using Yuki.BloggingService.Domain.Authors;
using Yuki.BloggingService.Domain.Common;
using Yuki.BloggingService.Domain.Posts;
using Yuki.BloggingService.Infrastructure;

namespace Yuki.BloggingService.Application.Commands.BlogPosts;

public class PublishBlogPostCommand(Guid blogPostId, Guid authorId)
{
    public Guid BlogPostId { get; } = blogPostId;
    public Guid AuthorId { get; } = authorId;
}

public class PublishBlogPostCommandHandler : ICommandHandler<PublishBlogPostCommand>
{
    private readonly IAggregateRepository _blogPostRepository;

    public PublishBlogPostCommandHandler(IAggregateRepository blogPostRepository)
    {
        _blogPostRepository = blogPostRepository;
    }

    public async Task HandleAsync(PublishBlogPostCommand command, CancellationToken cancellationToken = default)
    {
        var author = await _blogPostRepository.GetByIdAsync<Author>(command.AuthorId, cancellationToken);
        var blogPost = await _blogPostRepository.GetByIdAsync<BlogPost>(command.BlogPostId, cancellationToken);
        blogPost.Publish(author);
    }
}
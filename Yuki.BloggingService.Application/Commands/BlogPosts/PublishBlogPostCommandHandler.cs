using Yuki.BloggingService.Domain.Authors;
using Yuki.BloggingService.Domain.Common;
using Yuki.BloggingService.Domain.Posts;
using Yuki.BloggingService.Infrastructure;

namespace Yuki.BloggingService.Application.Commands.BlogPosts;

public class PublishBlogPostCommandHandler(IAggregateRepository blogPostRepository)
    : ICommandHandler<PublishBlogPostCommand>
{
    public async Task HandleAsync(PublishBlogPostCommand command, CancellationToken cancellationToken = default)
    {
        var author = await blogPostRepository.GetByIdAsync<Author>(command.AuthorId, cancellationToken);
        var blogPost = await blogPostRepository.GetByIdAsync<BlogPost>(command.BlogPostId, cancellationToken);
        blogPost.Publish(author);
        await blogPostRepository.SaveAsync(blogPost, cancellationToken);
    }
}
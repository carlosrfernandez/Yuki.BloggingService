using Moq;
using NUnit.Framework;
using Yuki.BloggingService.Application.Commands.BlogPosts;
using Yuki.BloggingService.Domain.Authors;
using Yuki.BloggingService.Domain.Common;
using Yuki.BloggingService.Domain.Posts;

namespace Yuki.BloggingService.Application.Tests.Commands.BlogPosts;

[TestFixture]
public class PublishBlogPostCommandHandlerTests
{
    [Test]
    public async Task HandleAsync_ShouldPublishBlogPost_WhenAuthorAuthorized()
    {
        var repositoryMock = new Mock<IAggregateRepository>();
        var authorId = Guid.NewGuid();
        var blogPostId = Guid.NewGuid();

        var author = TestHelpers.BuildRegisteredAuthor(authorId).BuildAuthorizedAuthor();
        
        var blogPost = TestHelpers.BuildDraftedBlogPost(blogPostId, authorId);

        repositoryMock
            .Setup(r => r.GetByIdAsync<Author>(authorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(author);
        repositoryMock
            .Setup(r => r.GetByIdAsync<BlogPost>(blogPostId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(blogPost);

        var handler = new PublishBlogPostCommandHandler(repositoryMock.Object);
        var command = new PublishBlogPostCommand(blogPostId, authorId);

        await handler.HandleAsync(command, CancellationToken.None);

        Assert.That(blogPost.IsPublished, Is.True);
        var @event = blogPost.GetUncommittedEvents().SingleOrDefault();
        Assert.That(@event, Is.TypeOf<BlogPostPublishedEvent>());
        var published = (BlogPostPublishedEvent)@event!;
        Assert.That(published.AuthorSurname, Is.EqualTo(author.Surname));

        repositoryMock.Verify(
            r => r.GetByIdAsync<Author>(authorId, It.IsAny<CancellationToken>()),
            Times.Once);
        repositoryMock.Verify(
            r => r.GetByIdAsync<BlogPost>(blogPostId, It.IsAny<CancellationToken>()),
            Times.Once);
    }
    
    [Test]
    public void HandleAsync_ShouldThrow_WhenAuthorNotAuthorized()
    {
        var repositoryMock = new Mock<IAggregateRepository>();
        var authorId = Guid.NewGuid();
        var blogPostId = Guid.NewGuid();

        var author = TestHelpers.BuildRegisteredAuthor(authorId); // Not authorized
        var blogPost = TestHelpers.BuildDraftedBlogPost(blogPostId, authorId);

        repositoryMock
            .Setup(r => r.GetByIdAsync<Author>(authorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(author);
        repositoryMock
            .Setup(r => r.GetByIdAsync<BlogPost>(blogPostId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(blogPost);

        var handler = new PublishBlogPostCommandHandler(repositoryMock.Object);
        var command = new PublishBlogPostCommand(blogPostId, authorId);

        Assert.That(async () => await handler.HandleAsync(command, CancellationToken.None),
            Throws.InstanceOf<InvalidOperationException>());

        repositoryMock.Verify(
            r => r.GetByIdAsync<Author>(authorId, It.IsAny<CancellationToken>()),
            Times.Once);
        repositoryMock.Verify(
            r => r.GetByIdAsync<BlogPost>(blogPostId, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}

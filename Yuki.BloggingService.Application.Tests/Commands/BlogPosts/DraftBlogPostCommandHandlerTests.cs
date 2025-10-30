using System.Diagnostics;
using Moq;
using NUnit.Framework;
using Yuki.BloggingService.Application.Commands.BlogPosts;
using Yuki.BloggingService.Domain.Authors;
using Yuki.BloggingService.Domain.Common;
using Yuki.BloggingService.Domain.Posts;

namespace Yuki.BloggingService.Application.Tests.Commands.BlogPosts;

[TestFixture]
public class DraftBlogPostCommandHandlerTests
{
    [Test]
    public async Task HandleAsync_ShouldDraftBlogPost_AndPersistThroughRepository()
    {
        var repositoryMock = new Mock<IAggregateRepository>();
        repositoryMock
            .Setup(r => r.GetByIdAsync<BlogPost>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BlogPost());

        BlogPost? savedBlogPost = null;
        repositoryMock
            .Setup(r => r.SaveAsync(It.IsAny<BlogPost>(), It.IsAny<CancellationToken>()))
            .Callback<BlogPost, CancellationToken>((post, _) => savedBlogPost = post)
            .Returns(Task.CompletedTask);

        var handler = new DraftBlogPostCommandHandler(repositoryMock.Object);
        var command = new DraftBlogPostCommand(
            AuthorId: Guid.NewGuid(),
            Title: "A first blog post",
            Description: "Summary",
            Content: "Content body");

        await handler.HandleAsync(command, CancellationToken.None);

        Assert.That(savedBlogPost, Is.Not.Null);
        Assert.That(savedBlogPost!.AuthorId, Is.EqualTo(command.AuthorId));
        Assert.That(savedBlogPost.Title, Is.EqualTo(command.Title));
        Assert.That(savedBlogPost.Description, Is.EqualTo(command.Description));
        Assert.That(savedBlogPost.Content, Is.EqualTo(command.Content));

        var @event = savedBlogPost.GetUncommittedEvents().SingleOrDefault();
        Assert.That(@event, Is.TypeOf<BlogPostDraftCreatedEvent>());

        repositoryMock.Verify(
            r => r.GetByIdAsync<BlogPost>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Once);
        repositoryMock.Verify(
            r => r.SaveAsync(It.IsAny<BlogPost>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public void HandleAsync_ShouldThrow_WhenTitleIsEmpty()
    {
        var repositoryMock = new Mock<IAggregateRepository>();
        repositoryMock.Setup(x => x.GetByIdAsync<Author>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestHelpers.BuildRegisteredAuthor(Guid.NewGuid()).BuildAuthorizedAuthor);
        repositoryMock.Setup(x => x.GetByIdAsync<BlogPost>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BlogPost());
        var handler = new DraftBlogPostCommandHandler(repositoryMock.Object);
        var command = new DraftBlogPostCommand(
            AuthorId: Guid.NewGuid(),
            Title: "",
            Description: "Summary",
            Content: "Content body");

        var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await handler.HandleAsync(command, CancellationToken.None));
        
        Assert.That(ex, Is.Not.Null);
        // This is to satisfy static analysis tools. Not sure why the line above is not removing this warning.
        Debug.Assert(ex != null, nameof(ex) + " != null");
        Assert.That(ex.Message, Is.Not.Null);
        
        Assert.That(ex.Message, Is.EqualTo("Title cannot be empty."));
    }
}

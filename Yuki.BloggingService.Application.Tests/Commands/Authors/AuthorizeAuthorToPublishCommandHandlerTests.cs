using Moq;
using NUnit.Framework;
using Yuki.BloggingService.Application.Commands.Authors;
using Yuki.BloggingService.Domain.Authors;
using Yuki.BloggingService.Domain.Common;

namespace Yuki.BloggingService.Application.Tests.Commands.Authors;

[TestFixture]
public class AuthorizeAuthorToPublishCommandHandlerTests
{
    [Test]
    public async Task HandleAsync_ShouldAuthorizeAuthor_AndPersistThroughRepository()
    {
        var repositoryMock = new Mock<IAggregateRepository>();
        var authorId = Guid.NewGuid();
        var author = TestHelpers.BuildRegisteredAuthor(authorId);

        repositoryMock
            .Setup(r => r.GetByIdAsync<Author>(authorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(author);

        Author? savedAuthor = null;
        repositoryMock
            .Setup(r => r.SaveAsync(It.IsAny<Author>(), It.IsAny<CancellationToken>()))
            .Callback<Author, CancellationToken>((a, _) => savedAuthor = a)
            .Returns(Task.CompletedTask);

        var handler = new AuthorizeAuthorToPublishCommandHandler(repositoryMock.Object);
        var command = new AuthorizeAuthorToPublishCommand { AuthorId = authorId };

        await handler.HandleAsync(command, CancellationToken.None);

        Assert.That(savedAuthor, Is.Not.Null);
        Assert.That(savedAuthor!.IsAuthorizedToPublish, Is.True);
        Assert.That(savedAuthor.Surname, Is.EqualTo(author.Surname));

        var @event = savedAuthor.GetUncommittedEvents().SingleOrDefault();
        Assert.That(@event, Is.TypeOf<AuthorAuthorizedToPublishEvent>());

        repositoryMock.Verify(
            r => r.GetByIdAsync<Author>(authorId, It.IsAny<CancellationToken>()),
            Times.Once);
        repositoryMock.Verify(
            r => r.SaveAsync(It.IsAny<Author>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}

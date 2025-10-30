using Moq;
using NUnit.Framework;
using Yuki.BloggingService.Application.Commands.Authors;
using Yuki.BloggingService.Domain.Authors;
using Yuki.BloggingService.Domain.Common;

namespace Yuki.BloggingService.Application.Tests.Commands.Authors;

[TestFixture]
public class RegisterNewAuthorCommandHandlerTests
{
    [Test]
    public async Task HandleAsync_ShouldRegisterAuthor_AndPersistThroughRepository()
    {
        var repositoryMock = new Mock<IAggregateRepository>();
        var author = new Author();
        repositoryMock
            .Setup(r => r.GetByIdAsync<Author>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(author);

        Author? savedAuthor = null;
        repositoryMock
            .Setup(r => r.SaveAsync(It.IsAny<Author>(), It.IsAny<CancellationToken>()))
            .Callback<Author, CancellationToken>((a, _) => savedAuthor = a)
            .Returns(Task.CompletedTask);

        var handler = new RegisterNewAuthorCommandHandler(repositoryMock.Object);
        var command = new RegisterNewAuthorCommand
        {
            Name = "Alice",
            Surname = "Smith",
            Email = "alice@example.com"
        };

        await handler.HandleAsync(command, CancellationToken.None);

        Assert.That(savedAuthor, Is.Not.Null);
        Assert.That(savedAuthor!.Name, Is.EqualTo(command.Name));
        Assert.That(savedAuthor.Surname, Is.EqualTo(command.Surname));
        Assert.That(savedAuthor.Email, Is.EqualTo(command.Email));

        var @event = savedAuthor.GetUncommittedEvents().SingleOrDefault();
        Assert.That(@event, Is.TypeOf<AuthorRegisteredEvent>());
        var registered = (AuthorRegisteredEvent)@event!;
        Assert.That(registered.Surname, Is.EqualTo(command.Surname));

        repositoryMock.Verify(
            r => r.GetByIdAsync<Author>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Once);
        repositoryMock.Verify(
            r => r.SaveAsync(It.IsAny<Author>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}

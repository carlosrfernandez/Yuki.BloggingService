using NUnit.Framework;
using Yuki.BloggingService.Domain.Authors;

namespace Yuki.BloggingService.Domain.Tests;

[TestFixture]
public class AuthorTests
{
    [Test]
    public void Register_ShouldSetState_AndRaiseEvent()
    {
        var author = new Author();

        author.Register("Carlos", "carlos@example.com");

        Assert.That(author.Name, Is.EqualTo("Carlos"));
        Assert.That(author.Email, Is.EqualTo("carlos@example.com"));

        var events = author.GetUncommittedEvents().ToList();
        Assert.That(events, Has.Count.EqualTo(1));
        Assert.That(events[0], Is.TypeOf<AuthorRegisteredEvent>());

        var registered = (AuthorRegisteredEvent)events[0];
        Assert.That(registered.Name, Is.EqualTo("Carlos"));
        Assert.That(registered.Email, Is.EqualTo("carlos@example.com"));
        Assert.That(registered.RegisteredAt, Is.Not.EqualTo(default(DateTimeOffset)));
    }

    [Test]
    public void Register_WhenAlreadyRegistered_ShouldThrow()
    {
        var author = CreateRegisteredAuthor();

        Assert.That(() => author.Register("Other", "other@example.com"),
            Throws.InstanceOf<InvalidOperationException>());
    }

    [Test]
    public void AuthorizeToPublishBlogPosts_WhenRegistered_ShouldRaiseEventAndSetFlag()
    {
        var author = CreateRegisteredAuthor();

        author.AuthorizeToPublishBlogPosts();

        Assert.That(author.IsAuthorizedToPublish, Is.True);
        var events = author.GetUncommittedEvents().ToList();
        Assert.That(events, Has.Count.EqualTo(1));
        Assert.That(events[0], Is.TypeOf<AuthorAuthorizedToPublishEvent>());
    }

    [Test]
    public void AuthorizeToPublishBlogPosts_WhenAlreadyAuthorized_ShouldNotRaiseAdditionalEvents()
    {
        var author = CreateRegisteredAuthor();
        author.AuthorizeToPublishBlogPosts();
        author.ClearUncommittedEvents();

        author.AuthorizeToPublishBlogPosts();

        Assert.That(author.GetUncommittedEvents(), Is.Empty);
    }

    private static Author CreateRegisteredAuthor()
    {
        var author = new Author();
        var registered = new AuthorRegisteredEvent(Guid.NewGuid(), "Existing", "existing@example.com",
            DateTimeOffset.UtcNow);
        author.LoadFromHistory([registered]);
        return author;
    }
}

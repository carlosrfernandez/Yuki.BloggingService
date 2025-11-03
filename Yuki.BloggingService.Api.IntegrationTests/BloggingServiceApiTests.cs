using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Yuki.BloggingService.Api.Controllers;
using Yuki.BloggingService.Domain.Authors;
using Yuki.BloggingService.Domain.Common;
using Yuki.BloggingService.Domain.Posts;
using Yuki.BloggingService.Infrastructure.Messaging;

namespace Yuki.BloggingService.Api.IntegrationTests;

[TestFixture]
[Parallelizable(ParallelScope.None)]
public class BloggingServiceApiTests
{
    private WebApplicationFactory<Program> _factory = null!;
    private HttpClient _client = null!;

    //TODO: Might be worth having this as a global config for all the app.
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [SetUp]
    public void SetUp()
    {
        _factory = new WebApplicationFactory<Program>();
        _client = _factory.CreateClient();
        _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    [TearDown]
    public void TearDown()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    [Test]
    public async Task RegisterAuthor_WithValidPayload_ReturnsAccepted()
    {
        var response = await _client.PostAsJsonAsync("/api/authors", new
        {
            Name = "Alice",
            Surname = "Smith",
            Email = "alice@example.com"
        });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Accepted));
    }

    [Test]
    public async Task RegisterAuthor_WithInvalidPayload_ReturnsBadRequest()
    {
        var response = await _client.PostAsJsonAsync("api/authors", new
        {
            Name = string.Empty,
            Surname = "Smith",
            Email = "alice@example.com"
        });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.InternalServerError));
    }

    [Test]
    public async Task GetBlogPost_WithMissingId_ReturnsNotFound()
    {
        var response = await _client.GetAsync($"/api/blogposts/{Guid.NewGuid()}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task BlogPost_Workflow_HappyPath_ReturnsSummaryAndAuthorInfo()
    {
        var (authorId, blogPostId) = await ExecuteHappyPathAsync();

        var summaryResponse = await _client.GetAsync($"/api/blogposts/{blogPostId}");
        Assert.That(summaryResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var summary =
            await summaryResponse.Content.ReadFromJsonAsync<BlogPostQueryController.BlogPostResponse>(JsonOptions);
        Assert.Multiple(() =>
        {
            Assert.That(summary, Is.Not.Null);
            Assert.That(summary?.BlogPostId, Is.EqualTo(blogPostId));
            Assert.That(summary?.AuthorId, Is.EqualTo(authorId));
            Assert.That(summary?.IsPublished, Is.True);
            Assert.That(summary?.AuthorName, Is.Null);
            Assert.That(summary?.AuthorSurname, Is.Null);
        });

        var authorResponse = await _client.GetAsync($"/api/blogposts/{blogPostId}?includeAuthorInfo=true");
        Assert.That(authorResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var authorInfo =
            await authorResponse.Content.ReadFromJsonAsync<BlogPostQueryController.BlogPostResponse>(JsonOptions);
        Assert.Multiple(() =>
        {
            Assert.That(authorInfo, Is.Not.Null);
            Assert.That(authorInfo?.AuthorName, Is.Not.Null);
            Assert.That(authorInfo?.AuthorName, Is.EqualTo("Alice"));
            Assert.That(authorInfo?.AuthorSurname, Is.EqualTo("Smith"));
            Assert.That(authorInfo?.IsPublished, Is.True);
        });
    }

    [Test]
    public async Task GetBlogPost_WithXmlAcceptHeader_ReturnsXmlPayload()
    {
        var (_, blogPostId) = await ExecuteHappyPathAsync();

        var request = new HttpRequestMessage(HttpMethod.Get, $"/api/blogposts/{blogPostId}?includeAuthorInfo=true");
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));

        var response = await _client.SendAsync(request);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(response.Content.Headers.ContentType?.MediaType, Is.EqualTo("application/xml"));
        var payload = await response.Content.ReadAsStringAsync();
        Assert.Multiple(() =>
        {
            Assert.That(payload, Does.Contain("<BlogPostResponse"));
            Assert.That(payload, Does.Contain("<AuthorName>Alice</AuthorName>"));
        });
    }

    private async Task<(Guid AuthorId, Guid BlogPostId)> ExecuteHappyPathAsync()
    {
        var authorRegisteredTask = WaitForEventAsync<AuthorRegisteredEvent>();

        var registerResponse = await _client.PostAsJsonAsync("api/authors", new
        {
            Name = "Alice",
            Surname = "Smith",
            Email = "alice@example.com"
        });

        Assert.That(registerResponse.StatusCode, Is.EqualTo(HttpStatusCode.Accepted));

        var authorRegistered = await authorRegisteredTask.WithTimeout();

        var authorizeResponse = await _client.PostAsync($"/api/authors/{authorRegistered.Id}/authorize",
            new StringContent("{}", Encoding.UTF8, "application/json"));
        Assert.That(authorizeResponse.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

        var draftCreatedTask = WaitForEventAsync<BlogPostDraftCreatedEvent>();

        var draftResponse = await _client.PostAsJsonAsync("/api/blogposts", new
        {
            AuthorId = authorRegistered.Id,
            Title = "Sample Title",
            Description = "Sample Description",
            Content = "Sample Content"
        });

        Assert.That(draftResponse.StatusCode, Is.EqualTo(HttpStatusCode.Accepted));

        var draftCreated = await draftCreatedTask.WithTimeout();

        var publishedTask = WaitForEventAsync<BlogPostPublishedEvent>();

        var publishResponse = await _client.PostAsJsonAsync($"/api/blogposts/{draftCreated.Id}/publish", new
        {
            AuthorId = authorRegistered.Id
        });
        Assert.That(publishResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        await publishedTask.WithTimeout();

        return (authorRegistered.Id, draftCreated.Id);
    }

    // Because we're in a CQRS application, we need to wait for events to be published to ensure projections are updated.
    // This is eventual consistency. 
    // For these tests, I've implemented a simple way to wait for specific events in the message bus,
    // in a real world test, we might want to wait for specific read-models to be populated... 
    // We use TaskCompletionSource and set the result to the event we ere waiting for.
    private Task<TEvent> WaitForEventAsync<TEvent>() where TEvent : class, IEvent
    {
        var services = _factory.Services;
        var eventBus = services.GetRequiredService<IEventBus>();
        var tcs = new TaskCompletionSource<TEvent>(TaskCreationOptions.RunContinuationsAsynchronously);
        IDisposable? subscription = null;
        subscription = eventBus.Subscribe<TEvent>(evt =>
        {
            // This should not be done in the real world. 
            // Disposing a subscription in the OnEvent handler.
            subscription?.Dispose();

            tcs.TrySetResult(evt);
            return Task.CompletedTask;
        });

        return tcs.Task;
    }
}
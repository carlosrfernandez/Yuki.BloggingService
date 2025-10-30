namespace Yuki.Queries.Projections.Full;

public record BlogPostWithAuthorInformationRecord
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset PublishedAt { get; init; }
    public Guid AuthorId { get; init; }
    public string AuthorName { get; init; } = string.Empty;
    public string AuthorSurname { get; init; } = string.Empty;
}
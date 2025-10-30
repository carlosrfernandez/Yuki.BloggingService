namespace Yuki.Queries.Projections.Summary;

public sealed record BlogPostDraftSummaryRecord(
    Guid Id,
    Guid AuthorId,
    string Title,
    string Description,
    string Content,
    DateTimeOffset CreatedAt, DateTimeOffset? PublishedAt);

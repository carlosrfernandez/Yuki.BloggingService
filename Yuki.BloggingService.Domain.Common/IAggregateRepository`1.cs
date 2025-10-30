namespace Yuki.BloggingService.Domain.Common;

/// <summary>
/// This is a very simple interface of an aggregate repository. In a real world case, we would add additional checks
/// for example, expected versioning to avoid concurrency issues.
/// </summary>
public interface IAggregateRepository
{
    Task SaveAsync<T>(T aggregate, CancellationToken cancellationToken = default) where T : AggregateRoot, new();
    Task<T> GetByIdAsync<T>(Guid id, CancellationToken cancellationToken = default) where T : AggregateRoot, new();
}
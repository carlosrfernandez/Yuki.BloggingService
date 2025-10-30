using Yuki.BloggingService.Domain.Authors;

namespace Yuki.Application;

public interface IAuthorAggregateRepository
{
    public Task<Author> GetAuthorByIdAsync(Guid authorId, CancellationToken cancellationToken = default);
}
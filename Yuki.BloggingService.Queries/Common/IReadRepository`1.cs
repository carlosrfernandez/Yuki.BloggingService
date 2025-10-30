using System.Collections.ObjectModel;

namespace Yuki.Queries.Common;

public interface IReadRepository<T>
{
    Task Upsert(Guid id, T entity);
    Task<bool> TryGetAsync(Guid id, out T entity, CancellationToken cancellationToken = default);
    Task<bool> TryUpdate(Guid id, Func<T, T> update);
    Task<ReadOnlyCollection<T>> GetAll();
}

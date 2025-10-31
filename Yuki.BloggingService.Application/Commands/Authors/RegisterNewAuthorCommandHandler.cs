using Yuki.BloggingService.Domain.Authors;
using Yuki.BloggingService.Domain.Common;
using Yuki.BloggingService.Infrastructure;

namespace Yuki.BloggingService.Application.Commands.Authors;

public class RegisterNewAuthorCommandHandler(IAggregateRepository repository)
    : ICommandHandler<RegisterNewAuthorCommand, Guid>
{
    private readonly IAggregateRepository _repository =
        repository ?? throw new ArgumentNullException(nameof(repository));

    public async Task<Guid> HandleAsync(RegisterNewAuthorCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        var id = Guid.NewGuid();
        var author = await _repository.GetByIdAsync<Author>(id, cancellationToken).ConfigureAwait(false);
        author.Register(command.Name, command.Surname, command.Email);
        await _repository.SaveAsync(author, cancellationToken).ConfigureAwait(false);
        return author.Id;
    }
}
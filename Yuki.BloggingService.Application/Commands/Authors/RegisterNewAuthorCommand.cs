namespace Yuki.BloggingService.Application.Commands.Authors;

public sealed record RegisterNewAuthorCommand
{
    public required string Name { get; init; }
    public required string Surname { get; init; }
    public required string Email { get; init; }
}

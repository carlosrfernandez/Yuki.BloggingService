using Yuki.BloggingService.Domain.Common;

namespace Yuki.BloggingService.Infrastructure.Tests;

public sealed record NameChanged(string Name) : IEvent;
using Microsoft.Extensions.DependencyInjection;
using Yuki.BloggingService.Domain.Common;
using Yuki.BloggingService.Infrastructure.Messaging;

namespace Yuki.BloggingService.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IAggregateRepository, InMemoryAggregateRepository>();
        services.AddSingleton<IEventBus, InMemoryEventBus>();
        return services;
    }
}
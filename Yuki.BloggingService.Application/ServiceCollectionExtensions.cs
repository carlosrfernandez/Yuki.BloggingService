using Microsoft.Extensions.DependencyInjection;
using Yuki.BloggingService.Application.Commands.Authors;
using Yuki.BloggingService.Application.Commands.BlogPosts;
using Yuki.BloggingService.Infrastructure;

namespace Yuki.BloggingService.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Register application services here
        services.AddScoped<ICommandHandler<RegisterNewAuthorCommand, Guid>, RegisterNewAuthorCommandHandler>();
        services.AddScoped<ICommandHandler<AuthorizeAuthorToPublishCommand, bool>, AuthorizeAuthorToPublishCommandHandler>();
        
        services.AddScoped<ICommandHandler<DraftBlogPostCommand, Guid>, DraftBlogPostCommandHandler>();
        services.AddScoped<ICommandHandler<PublishBlogPostCommand, bool>, PublishBlogPostCommandHandler>();
        
        return services;
    }
}
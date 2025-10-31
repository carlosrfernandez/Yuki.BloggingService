using Microsoft.Extensions.DependencyInjection;
using Yuki.Queries.Common;
using Yuki.Queries.Projections.Full;
using Yuki.Queries.Projections.Summary;

namespace Yuki.Queries;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddReadModelQueries(this IServiceCollection services)
    {
        services.AddSingleton<IReadRepository<BlogPostDraftSummaryRecord>, InMemoryReadRepository<BlogPostDraftSummaryRecord>>();
        services.AddSingleton<IReadRepository<BlogPostWithAuthorInformationRecord>, InMemoryReadRepository<BlogPostWithAuthorInformationRecord>>();
        services.AddSingleton<BlogPostSummaryProjection>();
        services.AddSingleton<BlogPostWithAuthorInformationProjection>();
        services.AddSingleton<IBlogPostQueries, BlogPostQueries>();
        return services;
    }
}

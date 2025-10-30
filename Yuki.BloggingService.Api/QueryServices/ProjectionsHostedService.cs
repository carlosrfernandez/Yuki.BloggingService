using Microsoft.Extensions.Hosting;
using Yuki.Queries.Projections.Full;
using Yuki.Queries.Projections.Summary;

namespace Yuki.BloggingService.Api.QueryServices;

public class ProjectionsHostedService(BlogPostSummaryProjection summaryProjection, BlogPostWithAuthorInfoRecordProjection fullProjection) : BackgroundService
{
    private readonly BlogPostSummaryProjection _summaryProjection = summaryProjection  ?? throw new ArgumentNullException(nameof(summaryProjection));
    private readonly BlogPostWithAuthorInfoRecordProjection _fullProjection = fullProjection ?? throw new ArgumentNullException(nameof(fullProjection));

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _summaryProjection.Start();
        _fullProjection.Start();
        return Task.CompletedTask;
    }

    public override Task StopAsync(CancellationToken stoppingToken)
    {
        _summaryProjection.Dispose();
        _fullProjection.Dispose();
        return Task.CompletedTask;
    }
}
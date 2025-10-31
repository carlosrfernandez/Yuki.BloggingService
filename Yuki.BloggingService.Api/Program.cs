using Microsoft.AspNetCore.Mvc;
using Yuki.BloggingService.Api.QueryServices;
using Yuki.BloggingService.Application;
using Yuki.BloggingService.Infrastructure;
using Yuki.Queries;
using Yuki.Queries.Common;
using Yuki.Queries.Projections.Full;
using Yuki.Queries.Projections.Summary;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers(options =>
    {
        options.RespectBrowserAcceptHeader = true;
        options.ReturnHttpNotAcceptable = true;
    })
    .AddXmlSerializerFormatters()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context => new BadRequestObjectResult(context.ModelState);
    });

builder.Services.AddInfrastructure();
builder.Services.AddApplicationServices();
builder.Services.AddReadModelQueries();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<BlogPostSummaryProjection>();
builder.Services.AddSingleton<BlogPostWithAuthorInformationProjection>();

builder.Services.AddSingleton<IBlogPostQueries, BlogPostQueries>();
builder.Services.AddHostedService<ProjectionsHostedService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();

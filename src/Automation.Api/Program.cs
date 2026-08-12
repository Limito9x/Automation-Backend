using Automation.Api;
using FastEndpoints;
using Automation.Agent.Infrastructure.Auth;
using Automation.SharedKernel.Extensions.Modules;
using Automation.SharedKernel.Extensions.Auth;
using Automation.SharedKernel.Extensions.Caching;
using Automation.SharedKernel.Extensions.Logging;
using Automation.SharedKernel.Extensions.Jobs;
using Automation.SharedKernel.Extensions.Observability;
using FastEndpoints.OpenApi;
using Scalar.AspNetCore;
using Automation.SharedKernel.Extensions.ExceptionHandling;

var builder = WebApplication.CreateSlimBuilder(args);
builder.AddCustomSerilog();
builder.AddOpenTelemetryServices();

builder.Services.AddFastEndpoints(ModuleRegistry.AllEndpoints)
    .OpenApiDocument(o =>
    {
        o.AutoTagPathSegmentIndex = 0;
        o.ShortSchemaNames = true;
    });

builder.Services.AddCurrentUserProvider();

builder.AddModules(ModuleRegistry.All);

builder.Services.AddJobServices(builder.Configuration);

builder.Services.AddOpenApi();

builder.Services.AddRedisCache(builder.Configuration);

builder.Services.AddRateLimitingServices(builder.Configuration);

builder.Services.AddGlobalExceptionHandling();

builder.Services.AddAuthentication();
builder.Services.AddAuthorization();
builder.Services.AddSignalR();

var app = builder.Build();

app.UseGlobalExceptionHandling();

app.UseCustomSerilogRequestLogging();

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();
app.UseAgentAuthentication();

app.UseJobMiddleware();

app.UseFastEndpoints(c =>
{
    c.Endpoints.RoutePrefix = "api";
    c.Security.PermissionsClaimType = "Permission";
});

app.MapHub<Automation.Notifications.Features.Notifications.NotificationHub>("/hubs/notifications");

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options
            .WithTitle("Template API Reference")
            .WithTheme(ScalarTheme.DeepSpace)
            .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
    });
}

await app.RunAsync();
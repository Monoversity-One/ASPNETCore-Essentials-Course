using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

// Add DbContext for health check demonstration
builder.Services.AddDbContext<AppDb>(opt =>
    opt.UseSqlite("Data Source=health.db"));

// Configure Health Checks
builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy("Application is running"))
    .AddCheck<CustomHealthCheck>("custom")
    .AddDbContextCheck<AppDb>("database")
    .AddUrlGroup(new Uri("https://www.google.com"), "external_service", timeout: TimeSpan.FromSeconds(3))
    .AddCheck("memory", () =>
    {
        var allocated = GC.GetTotalMemory(forceFullCollection: false);
        var threshold = 1024L * 1024L * 1024L; // 1 GB
        return allocated < threshold
            ? HealthCheckResult.Healthy($"Memory usage: {allocated / 1024 / 1024} MB")
            : HealthCheckResult.Degraded($"High memory usage: {allocated / 1024 / 1024} MB");
    });

// Configure OpenTelemetry for observability
const string serviceName = "HealthChecksObservability";

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService(serviceName))
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddSource(serviceName)
        .AddConsoleExporter())
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddMeter(serviceName)
        .AddConsoleExporter());

// Create custom ActivitySource and Meter for manual instrumentation
var activitySource = new ActivitySource(serviceName);
var meter = new System.Diagnostics.Metrics.Meter(serviceName);
var requestCounter = meter.CreateCounter<int>("custom.requests", description: "Counts custom requests");

builder.Services.AddSingleton(activitySource);
builder.Services.AddSingleton(requestCounter);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDb>();
    db.Database.EnsureCreated();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Map health check endpoints
// Basic health check endpoint
app.MapHealthChecks("/health");

// Detailed health check with custom response
app.MapHealthChecks("/health/detailed", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var result = System.Text.Json.JsonSerializer.Serialize(new
        {
            status = report.Status.ToString(),
            duration = report.TotalDuration,
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description,
                duration = e.Value.Duration,
                exception = e.Value.Exception?.Message,
                data = e.Value.Data
            })
        });
        await context.Response.WriteAsync(result);
    }
});

// Liveness probe (for Kubernetes)
app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("live")
});

// Readiness probe (for Kubernetes)
app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});

// Demo endpoints with custom tracing and metrics
app.MapGet("/", () => "Health Checks & Observability module - Visit /health, /health/detailed, /swagger");

app.MapGet("/api/data", async (AppDb db, ActivitySource activitySource, System.Diagnostics.Metrics.Counter<int> counter) =>
{
    // Create custom span for tracing
    using var activity = activitySource.StartActivity("GetData");
    activity?.SetTag("operation", "fetch_data");

    // Increment custom metric
    counter.Add(1);

    await Task.Delay(Random.Shared.Next(10, 100)); // Simulate work
    
    var items = await db.Items.ToListAsync();
    
    activity?.SetTag("item_count", items.Count);
    
    return Results.Ok(new { count = items.Count, items });
});

app.MapPost("/api/data", async (DataItem item, AppDb db, ActivitySource activitySource) =>
{
    using var activity = activitySource.StartActivity("CreateData");
    activity?.SetTag("item.name", item.Name);

    db.Items.Add(item);
    await db.SaveChangesAsync();

    return Results.Created($"/api/data/{item.Id}", item);
});

// Endpoint that demonstrates error tracking
app.MapGet("/api/error", (ActivitySource activitySource) =>
{
    using var activity = activitySource.StartActivity("ErrorDemo");
    activity?.SetStatus(ActivityStatusCode.Error, "Intentional error for demo");
    
    throw new InvalidOperationException("This is a demo error for observability");
});

app.Run();

// Custom health check implementation
public class CustomHealthCheck : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        // Simulate custom business logic health check
        var isHealthy = DateTime.UtcNow.Second % 2 == 0;

        if (isHealthy)
        {
            return Task.FromResult(HealthCheckResult.Healthy("Custom check passed"));
        }

        return Task.FromResult(
            HealthCheckResult.Degraded("Custom check degraded", 
                data: new Dictionary<string, object> { ["timestamp"] = DateTime.UtcNow }));
    }
}

// Database context
public class AppDb : DbContext
{
    public AppDb(DbContextOptions<AppDb> options) : base(options) { }
    public DbSet<DataItem> Items => Set<DataItem>();
}

public class DataItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}


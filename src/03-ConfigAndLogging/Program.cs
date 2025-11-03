var builder = WebApplication.CreateBuilder(args);

// Demonstrate configuration providers
builder.Configuration
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables(prefix: "ESSENTIALS_");

// Demonstrate logging configuration
builder.Logging
    .ClearProviders()
    .AddConsole()
    .AddDebug();

var app = builder.Build();

app.MapGet("/config", (IConfiguration cfg) =>
{
    var setting = cfg["Sample:Message"] ?? "(missing)";
    return Results.Ok(new { setting });
});

app.MapGet("/log", (ILoggerFactory factory) =>
{
    var logger = factory.CreateLogger("Demo");
    logger.LogInformation("This is an information log at {time}", DateTimeOffset.UtcNow);
    return Results.Ok(new { logged = true });
});

app.MapGet("/", () => "Configuration and Logging module");

app.Run();

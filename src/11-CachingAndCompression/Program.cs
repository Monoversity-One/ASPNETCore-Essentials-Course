using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Caching.Memory;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMemoryCache();
builder.Services.AddResponseCaching();
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
});

var app = builder.Build();

app.UseResponseCompression();
app.UseResponseCaching();

app.MapGet("/data", async (IMemoryCache cache) =>
{
    if (!cache.TryGetValue("payload", out string payload))
    {
        payload = new string('X', 10_000);
        cache.Set("payload", payload, TimeSpan.FromSeconds(30));
    }
    return Results.Ok(new { length = payload.Length });
}).CacheOutput();

app.MapGet("/heavy", () => new string('Y', 50_000));

app.Run();

static class CachingExtensions
{
    public static RouteHandlerBuilder CacheOutput(this RouteHandlerBuilder builder)
        => builder.WithMetadata(new ResponseCacheAttribute { Duration = 30, Location = ResponseCacheLocation.Any });
}

public sealed class ResponseCacheAttribute : Attribute
{
    public int Duration { get; set; }
    public ResponseCacheLocation Location { get; set; }
}

public enum ResponseCacheLocation { Any, Client, None }

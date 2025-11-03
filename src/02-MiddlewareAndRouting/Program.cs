var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRouting();

var app = builder.Build();

// Custom middleware: simple request logging
app.Use(async (ctx, next) =>
{
    var sw = System.Diagnostics.Stopwatch.StartNew();
    Console.WriteLine($"--> {ctx.Request.Method} {ctx.Request.Path}");
    await next();
    sw.Stop();
    Console.WriteLine($"<-- {ctx.Response.StatusCode} ({sw.ElapsedMilliseconds}ms)");
});

// Branching pipeline by path
app.Map("/branch", branchApp =>
{
    branchApp.Run(async ctx =>
    {
        ctx.Response.ContentType = "text/plain";
        await ctx.Response.WriteAsync("Hello from a branched pipeline");
    });
});

// Endpoint routing with constraints
app.MapGet("/items/{id:int:min(1)}", (int id) => Results.Ok(new { id, at = DateTimeOffset.UtcNow }))
   .WithName("GetItem");

// Conventional middleware after endpoints (e.g., not recommended but for teaching)
app.Use(async (ctx, next) =>
{
    // Add a header
    ctx.Response.Headers.TryAdd("X-Sample", "MiddlewareAndRouting");
    await next();
});

app.MapGet("/", () => "Middleware and Routing module");

app.Run();

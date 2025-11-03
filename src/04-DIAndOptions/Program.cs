using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<GreetingOptions>(builder.Configuration.GetSection("Greeting"));

builder.Services.AddSingleton<ITimeProvider, SystemTimeProvider>();
builder.Services.AddScoped<GreetingService>();

var app = builder.Build();

app.MapGet("/greet/{name}", (string name, GreetingService svc) => svc.Greet(name));
app.MapGet("/options", (IOptionsSnapshot<GreetingOptions> opts) => Results.Ok(opts.Value));
app.MapGet("/", () => "DI and Options module");

app.Run();

public record GreetingOptions
{
    public string Prefix { get; init; } = "Hello";
}

public interface ITimeProvider
{
    DateTimeOffset Now { get; }
}

public sealed class SystemTimeProvider : ITimeProvider
{
    public DateTimeOffset Now => DateTimeOffset.Now;
}

public sealed class GreetingService
{
    private readonly ITimeProvider _time;
    private readonly GreetingOptions _opts;

    public GreetingService(ITimeProvider time, IOptionsSnapshot<GreetingOptions> opts)
    {
        _time = time;
        _opts = opts.Value;
    }

    public IResult Greet(string name) => Results.Ok(new
    {
        message = $"{_opts.Prefix}, {name}!",
        at = _time.Now
    });
}

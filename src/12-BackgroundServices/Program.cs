var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHostedService<TimedPrinter>();

var app = builder.Build();

app.MapGet("/", () => "Background services module running. Check console logs.");

app.Run();

public sealed class TimedPrinter : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            Console.WriteLine($"Heartbeat at {DateTimeOffset.Now:O}");
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }
}

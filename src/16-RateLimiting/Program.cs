using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Configure different rate limiting policies
builder.Services.AddRateLimiter(options =>
{
    // 1. Fixed Window Limiter - allows X requests per time window
    options.AddPolicy("fixed", context =>
        RateLimitPartition.GetFixedWindowLimiter("fixed", _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 5,
            Window = TimeSpan.FromSeconds(10),
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            QueueLimit = 2
        }));

    // 2. Sliding Window Limiter - smoother than fixed window
    options.AddPolicy("sliding", context =>
        RateLimitPartition.GetSlidingWindowLimiter("sliding", _ => new SlidingWindowRateLimiterOptions
        {
            PermitLimit = 10,
            Window = TimeSpan.FromSeconds(30),
            SegmentsPerWindow = 3,
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            QueueLimit = 2
        }));

    // 3. Token Bucket Limiter - allows bursts
    options.AddPolicy("token", context =>
        RateLimitPartition.GetTokenBucketLimiter("token", _ => new TokenBucketRateLimiterOptions
        {
            TokenLimit = 10,
            ReplenishmentPeriod = TimeSpan.FromSeconds(10),
            TokensPerPeriod = 5,
            AutoReplenishment = true,
            QueueLimit = 0
        }));

    // 4. Concurrency Limiter - limits concurrent requests
    options.AddPolicy("concurrency", context =>
        RateLimitPartition.GetConcurrencyLimiter("concurrency", _ => new ConcurrencyLimiterOptions
        {
            PermitLimit = 3,
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            QueueLimit = 5
        }));

    // 5. Per-user rate limiting using partition
    options.AddPolicy("per-user", context =>
    {
        var username = context.User.Identity?.Name ?? "anonymous";
        
        return RateLimitPartition.GetFixedWindowLimiter(username, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 3,
            Window = TimeSpan.FromSeconds(10),
            QueueLimit = 0
        });
    });

    // 6. Per-IP rate limiting
    options.AddPolicy("per-ip", context =>
    {
        var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        
        return RateLimitPartition.GetSlidingWindowLimiter(ipAddress, _ => new SlidingWindowRateLimiterOptions
        {
            PermitLimit = 20,
            Window = TimeSpan.FromMinutes(1),
            SegmentsPerWindow = 4,
            QueueLimit = 0
        });
    });

    // Global rejection response
    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;

        TimeSpan? retryAfterValue = null;
        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
        {
            retryAfterValue = retryAfter;
            context.HttpContext.Response.Headers.RetryAfter = retryAfter.TotalSeconds.ToString();
        }

        await context.HttpContext.Response.WriteAsJsonAsync(new
        {
            error = "Too many requests",
            message = "Rate limit exceeded. Please try again later.",
            retryAfter = retryAfterValue?.TotalSeconds
        }, cancellationToken);
    };
});

var app = builder.Build();

// Enable rate limiting middleware
app.UseRateLimiter();

// Home page with documentation
app.MapGet("/", () => Results.Content("""
    <!DOCTYPE html>
    <html>
    <head>
        <title>Rate Limiting Demo</title>
        <style>
            body { font-family: Arial, sans-serif; max-width: 1000px; margin: 50px auto; padding: 20px; }
            .endpoint { margin: 20px 0; padding: 15px; border: 1px solid #ddd; border-radius: 5px; }
            .endpoint h3 { margin-top: 0; color: #0066cc; }
            code { background: #f4f4f4; padding: 2px 6px; border-radius: 3px; }
            button { padding: 10px 20px; margin: 5px; cursor: pointer; background: #0066cc; color: white; border: none; border-radius: 4px; }
            button:hover { background: #0052a3; }
            #results { margin-top: 20px; padding: 15px; background: #f9f9f9; border-radius: 5px; min-height: 100px; }
            .success { color: green; }
            .error { color: red; }
        </style>
    </head>
    <body>
        <h1>Rate Limiting Demo</h1>
        <p>This demo showcases different rate limiting strategies in ASP.NET Core.</p>

        <div class="endpoint">
            <h3>1. Fixed Window Limiter</h3>
            <p><strong>Limit:</strong> 5 requests per 10 seconds</p>
            <p><strong>Endpoint:</strong> <code>GET /api/fixed</code></p>
            <button onclick="testEndpoint('/api/fixed', 'fixed')">Test Fixed Window (Click Multiple Times)</button>
        </div>

        <div class="endpoint">
            <h3>2. Sliding Window Limiter</h3>
            <p><strong>Limit:</strong> 10 requests per 30 seconds (3 segments)</p>
            <p><strong>Endpoint:</strong> <code>GET /api/sliding</code></p>
            <button onclick="testEndpoint('/api/sliding', 'sliding')">Test Sliding Window</button>
        </div>

        <div class="endpoint">
            <h3>3. Token Bucket Limiter</h3>
            <p><strong>Limit:</strong> 10 tokens, replenish 5 tokens every 10 seconds</p>
            <p><strong>Endpoint:</strong> <code>GET /api/token</code></p>
            <button onclick="testEndpoint('/api/token', 'token')">Test Token Bucket</button>
        </div>

        <div class="endpoint">
            <h3>4. Concurrency Limiter</h3>
            <p><strong>Limit:</strong> 3 concurrent requests</p>
            <p><strong>Endpoint:</strong> <code>GET /api/concurrency</code></p>
            <button onclick="testConcurrency()">Test Concurrency (Sends 5 parallel requests)</button>
        </div>

        <div class="endpoint">
            <h3>5. Per-IP Rate Limiting</h3>
            <p><strong>Limit:</strong> 20 requests per minute per IP</p>
            <p><strong>Endpoint:</strong> <code>GET /api/per-ip</code></p>
            <button onclick="testEndpoint('/api/per-ip', 'per-ip')">Test Per-IP Limiting</button>
        </div>

        <div id="results">
            <strong>Results will appear here...</strong>
        </div>

        <script>
            let requestCount = 0;

            async function testEndpoint(url, name) {
                requestCount++;
                const start = Date.now();
                
                try {
                    const response = await fetch(url);
                    const duration = Date.now() - start;
                    const data = await response.json();
                    
                    const resultDiv = document.getElementById('results');
                    const className = response.ok ? 'success' : 'error';
                    const retryAfter = response.headers.get('Retry-After');
                    
                    resultDiv.innerHTML = `
                        <div class="${className}">
                            <strong>Request #${requestCount} to ${name}:</strong><br>
                            Status: ${response.status} ${response.statusText}<br>
                            Duration: ${duration}ms<br>
                            ${retryAfter ? `Retry After: ${retryAfter}s<br>` : ''}
                            Response: ${JSON.stringify(data, null, 2)}
                        </div>
                    ` + resultDiv.innerHTML;
                } catch (error) {
                    const resultDiv = document.getElementById('results');
                    resultDiv.innerHTML = `<div class="error">Error: ${error.message}</div>` + resultDiv.innerHTML;
                }
            }

            async function testConcurrency() {
                const promises = [];
                for (let i = 0; i < 5; i++) {
                    promises.push(testEndpoint('/api/concurrency', 'concurrency'));
                }
                await Promise.all(promises);
            }
        </script>
    </body>
    </html>
    """, "text/html"));

// Endpoints with different rate limiting policies

app.MapGet("/api/fixed", () => new
{
    message = "Fixed window limiter",
    timestamp = DateTime.UtcNow,
    info = "5 requests per 10 seconds"
}).RequireRateLimiting("fixed");

app.MapGet("/api/sliding", () => new
{
    message = "Sliding window limiter",
    timestamp = DateTime.UtcNow,
    info = "10 requests per 30 seconds"
}).RequireRateLimiting("sliding");

app.MapGet("/api/token", () => new
{
    message = "Token bucket limiter",
    timestamp = DateTime.UtcNow,
    info = "10 tokens, +5 every 10 seconds"
}).RequireRateLimiting("token");

app.MapGet("/api/concurrency", async () =>
{
    // Simulate slow operation
    await Task.Delay(2000);
    return new
    {
        message = "Concurrency limiter",
        timestamp = DateTime.UtcNow,
        info = "Max 3 concurrent requests"
    };
}).RequireRateLimiting("concurrency");

app.MapGet("/api/per-ip", () => new
{
    message = "Per-IP rate limiting",
    timestamp = DateTime.UtcNow,
    info = "20 requests per minute per IP"
}).RequireRateLimiting("per-ip");

// Endpoint without rate limiting for comparison
app.MapGet("/api/unlimited", () => new
{
    message = "No rate limiting",
    timestamp = DateTime.UtcNow,
    info = "Unlimited requests"
});

// Admin endpoint to check rate limiter statistics
app.MapGet("/api/stats", () => new
{
    message = "Rate limiter is active",
    policies = new[]
    {
        "fixed - 5 req/10s",
        "sliding - 10 req/30s",
        "token - 10 tokens, +5/10s",
        "concurrency - 3 concurrent",
        "per-ip - 20 req/min per IP"
    }
});

app.Run();


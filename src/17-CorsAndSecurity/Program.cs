var builder = WebApplication.CreateBuilder(args);

// Configure CORS policies
builder.Services.AddCors(options =>
{
    // 1. Allow all - for development only (NOT for production)
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });

    // 2. Specific origins - recommended for production
    options.AddPolicy("AllowSpecificOrigins", policy =>
    {
        policy.WithOrigins("https://example.com", "https://app.example.com")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials(); // Required for cookies/auth
    });

    // 3. Restricted policy - specific methods and headers
    options.AddPolicy("RestrictedPolicy", policy =>
    {
        policy.WithOrigins("https://trusted-site.com")
              .WithMethods("GET", "POST")
              .WithHeaders("Content-Type", "Authorization")
              .WithExposedHeaders("X-Custom-Header")
              .SetPreflightMaxAge(TimeSpan.FromMinutes(10));
    });

    // 4. Default policy
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:5173") // Common dev ports
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

// Security Headers Middleware
app.Use(async (context, next) =>
{
    // Prevent clickjacking attacks
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    
    // Prevent MIME type sniffing
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    
    // Enable XSS protection (legacy browsers)
    context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
    
    // Content Security Policy - prevents XSS and injection attacks
    context.Response.Headers.Append("Content-Security-Policy",
        "default-src 'self'; " +
        "script-src 'self' 'unsafe-inline' https://cdn.jsdelivr.net; " +
        "style-src 'self' 'unsafe-inline'; " +
        "img-src 'self' data: https:; " +
        "font-src 'self' data:; " +
        "connect-src 'self'; " +
        "frame-ancestors 'none';");
    
    // Referrer Policy - controls referrer information
    context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
    
    // Permissions Policy (formerly Feature Policy)
    context.Response.Headers.Append("Permissions-Policy",
        "geolocation=(), " +
        "microphone=(), " +
        "camera=(), " +
        "payment=(), " +
        "usb=()");
    
    // Strict Transport Security - enforce HTTPS
    if (context.Request.IsHttps)
    {
        context.Response.Headers.Append("Strict-Transport-Security",
            "max-age=31536000; includeSubDomains; preload");
    }

    await next();
});

// Enable CORS middleware (must be before endpoints)
app.UseCors();

// Home page with interactive CORS tester
app.MapGet("/", () => Results.Content("""
    <!DOCTYPE html>
    <html>
    <head>
        <title>CORS & Security Headers Demo</title>
        <style>
            body { font-family: Arial, sans-serif; max-width: 1000px; margin: 50px auto; padding: 20px; }
            .section { margin: 30px 0; padding: 20px; border: 1px solid #ddd; border-radius: 5px; }
            .section h2 { margin-top: 0; color: #0066cc; }
            code { background: #f4f4f4; padding: 2px 6px; border-radius: 3px; font-family: monospace; }
            pre { background: #f4f4f4; padding: 15px; border-radius: 5px; overflow-x: auto; }
            button { padding: 10px 20px; margin: 5px; cursor: pointer; background: #0066cc; color: white; border: none; border-radius: 4px; }
            button:hover { background: #0052a3; }
            .result { margin-top: 10px; padding: 10px; border-radius: 4px; }
            .success { background: #d4edda; color: #155724; }
            .error { background: #f8d7da; color: #721c24; }
            table { width: 100%; border-collapse: collapse; margin-top: 10px; }
            th, td { padding: 8px; text-align: left; border-bottom: 1px solid #ddd; }
            th { background: #f4f4f4; }
        </style>
    </head>
    <body>
        <h1>CORS & Security Headers Demo</h1>

        <div class="section">
            <h2>Security Headers</h2>
            <p>This application sets the following security headers on all responses:</p>
            <table>
                <tr><th>Header</th><th>Value</th><th>Purpose</th></tr>
                <tr><td>X-Frame-Options</td><td>DENY</td><td>Prevent clickjacking</td></tr>
                <tr><td>X-Content-Type-Options</td><td>nosniff</td><td>Prevent MIME sniffing</td></tr>
                <tr><td>X-XSS-Protection</td><td>1; mode=block</td><td>Enable XSS filter</td></tr>
                <tr><td>Content-Security-Policy</td><td>...</td><td>Prevent XSS/injection</td></tr>
                <tr><td>Referrer-Policy</td><td>strict-origin-when-cross-origin</td><td>Control referrer info</td></tr>
                <tr><td>Permissions-Policy</td><td>...</td><td>Control browser features</td></tr>
                <tr><td>Strict-Transport-Security</td><td>max-age=31536000</td><td>Enforce HTTPS</td></tr>
            </table>
            <button onclick="checkSecurityHeaders()">Check Current Security Headers</button>
            <div id="securityResult"></div>
        </div>

        <div class="section">
            <h2>CORS Policies</h2>
            <p>This application has multiple CORS policies configured:</p>
            <ul>
                <li><strong>Default Policy:</strong> Allows localhost:3000, localhost:5173 (dev)</li>
                <li><strong>AllowAll:</strong> Allows any origin (dev only)</li>
                <li><strong>AllowSpecificOrigins:</strong> Allows specific production domains</li>
                <li><strong>RestrictedPolicy:</strong> Strict policy with limited methods/headers</li>
            </ul>
        </div>

        <div class="section">
            <h2>Test CORS Endpoints</h2>
            
            <h3>1. Default CORS Policy</h3>
            <button onclick="testCors('/api/data')">Test Default Policy</button>
            <div id="defaultResult"></div>

            <h3>2. Allow All Policy</h3>
            <button onclick="testCors('/api/public')">Test Allow All</button>
            <div id="publicResult"></div>

            <h3>3. Specific Origins Policy</h3>
            <button onclick="testCors('/api/restricted')">Test Specific Origins</button>
            <div id="restrictedResult"></div>

            <h3>4. No CORS Policy</h3>
            <button onclick="testCors('/api/no-cors')">Test No CORS</button>
            <div id="noCorsResult"></div>
        </div>

        <div class="section">
            <h2>Preflight Request Test</h2>
            <p>Test CORS preflight (OPTIONS request) for custom headers:</p>
            <button onclick="testPreflight()">Send Preflight Request</button>
            <div id="preflightResult"></div>
        </div>

        <script>
            async function checkSecurityHeaders() {
                try {
                    const response = await fetch('/api/data');
                    const headers = {};
                    response.headers.forEach((value, key) => {
                        if (key.toLowerCase().startsWith('x-') || 
                            key.toLowerCase().includes('security') ||
                            key.toLowerCase().includes('policy') ||
                            key.toLowerCase().includes('referrer')) {
                            headers[key] = value;
                        }
                    });
                    
                    document.getElementById('securityResult').innerHTML = 
                        '<div class="result success"><strong>Security Headers:</strong><pre>' + 
                        JSON.stringify(headers, null, 2) + '</pre></div>';
                } catch (error) {
                    document.getElementById('securityResult').innerHTML = 
                        '<div class="result error">Error: ' + error.message + '</div>';
                }
            }

            async function testCors(endpoint) {
                const resultId = endpoint.split('/').pop() + 'Result';
                try {
                    const response = await fetch(endpoint);
                    const data = await response.json();
                    const corsHeaders = {
                        'Access-Control-Allow-Origin': response.headers.get('Access-Control-Allow-Origin'),
                        'Access-Control-Allow-Methods': response.headers.get('Access-Control-Allow-Methods'),
                        'Access-Control-Allow-Headers': response.headers.get('Access-Control-Allow-Headers'),
                    };
                    
                    document.getElementById(resultId).innerHTML = 
                        '<div class="result success">' +
                        '<strong>Success!</strong><br>' +
                        'Data: ' + JSON.stringify(data) + '<br>' +
                        'CORS Headers: <pre>' + JSON.stringify(corsHeaders, null, 2) + '</pre>' +
                        '</div>';
                } catch (error) {
                    document.getElementById(resultId).innerHTML = 
                        '<div class="result error"><strong>Error:</strong> ' + error.message + '</div>';
                }
            }

            async function testPreflight() {
                try {
                    const response = await fetch('/api/restricted', {
                        method: 'POST',
                        headers: {
                            'Content-Type': 'application/json',
                            'X-Custom-Header': 'test-value'
                        },
                        body: JSON.stringify({ test: 'data' })
                    });
                    
                    const data = await response.json();
                    document.getElementById('preflightResult').innerHTML = 
                        '<div class="result success">' +
                        '<strong>Preflight succeeded!</strong><br>' +
                        'Response: ' + JSON.stringify(data) +
                        '</div>';
                } catch (error) {
                    document.getElementById('preflightResult').innerHTML = 
                        '<div class="result error">' +
                        '<strong>Preflight failed:</strong> ' + error.message +
                        '<br>This is expected if origin is not in the allowed list.' +
                        '</div>';
                }
            }
        </script>
    </body>
    </html>
    """, "text/html"));

// API Endpoints with different CORS policies

// Default CORS policy (from localhost dev servers)
app.MapGet("/api/data", () => new
{
    message = "This endpoint uses the default CORS policy",
    timestamp = DateTime.UtcNow,
    policy = "Default (localhost:3000, localhost:5173)"
}).RequireCors();

// Allow all origins (development only)
app.MapGet("/api/public", () => new
{
    message = "This endpoint allows all origins",
    timestamp = DateTime.UtcNow,
    policy = "AllowAll"
}).RequireCors("AllowAll");

// Specific origins only
app.MapGet("/api/restricted", () => new
{
    message = "This endpoint only allows specific origins",
    timestamp = DateTime.UtcNow,
    policy = "AllowSpecificOrigins"
}).RequireCors("AllowSpecificOrigins");

app.MapPost("/api/restricted", (object data) => new
{
    message = "POST request received",
    data,
    timestamp = DateTime.UtcNow
}).RequireCors("RestrictedPolicy");

// No CORS policy - will fail from different origins
app.MapGet("/api/no-cors", () => new
{
    message = "This endpoint has no CORS policy",
    timestamp = DateTime.UtcNow,
    policy = "None - will fail from different origins"
});

// Endpoint to demonstrate credentials
app.MapGet("/api/with-credentials", (HttpContext context) =>
{
    var cookie = context.Request.Cookies["demo-cookie"];
    return new
    {
        message = "This endpoint supports credentials",
        cookie = cookie ?? "No cookie found",
        timestamp = DateTime.UtcNow
    };
}).RequireCors("AllowSpecificOrigins");

app.Run();


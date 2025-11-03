using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Configure ProblemDetails
builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = context =>
    {
        context.ProblemDetails.Instance = $"{context.HttpContext.Request.Method} {context.HttpContext.Request.Path}";
        context.ProblemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;
        context.ProblemDetails.Extensions["timestamp"] = DateTime.UtcNow;
    };
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

// Global exception handler (ASP.NET Core 8+)
app.UseExceptionHandler(exceptionHandlerApp =>
{
    exceptionHandlerApp.Run(async context =>
    {
        var exceptionHandlerFeature = context.Features.Get<IExceptionHandlerFeature>();
        var exception = exceptionHandlerFeature?.Error;

        var problemDetails = exception switch
        {
            ValidationException validationEx => new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Validation Error",
                Detail = validationEx.Message,
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1"
            },
            UnauthorizedAccessException => new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "Unauthorized",
                Detail = "You are not authorized to access this resource",
                Type = "https://tools.ietf.org/html/rfc7235#section-3.1"
            },
            KeyNotFoundException notFoundEx => new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Resource Not Found",
                Detail = notFoundEx.Message,
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4"
            },
            InvalidOperationException invalidOpEx => new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Conflict",
                Detail = invalidOpEx.Message,
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.8"
            },
            _ => new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "An error occurred",
                Detail = app.Environment.IsDevelopment() ? exception?.Message : "An unexpected error occurred",
                Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1"
            }
        };

        problemDetails.Instance = $"{context.Request.Method} {context.Request.Path}";
        problemDetails.Extensions["traceId"] = context.TraceIdentifier;
        problemDetails.Extensions["timestamp"] = DateTime.UtcNow;

        if (app.Environment.IsDevelopment() && exception != null)
        {
            problemDetails.Extensions["exception"] = new
            {
                type = exception.GetType().Name,
                message = exception.Message,
                stackTrace = exception.StackTrace
            };
        }

        context.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/problem+json";

        await context.Response.WriteAsJsonAsync(problemDetails);
    });
});

// Status code pages for non-exception errors (404, etc.)
app.UseStatusCodePages(async context =>
{
    var response = context.HttpContext.Response;
    
    if (response.StatusCode == StatusCodes.Status404NotFound)
    {
        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status404NotFound,
            Title = "Not Found",
            Detail = $"The requested resource '{context.HttpContext.Request.Path}' was not found",
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
            Instance = $"{context.HttpContext.Request.Method} {context.HttpContext.Request.Path}"
        };

        response.ContentType = "application/problem+json";
        await response.WriteAsJsonAsync(problemDetails);
    }
});

app.MapGet("/", () => Results.Content("""
    <!DOCTYPE html>
    <html>
    <head>
        <title>Error Handling & Problem Details Demo</title>
        <style>
            body { font-family: Arial, sans-serif; max-width: 1000px; margin: 50px auto; padding: 20px; }
            .section { margin: 30px 0; padding: 20px; border: 1px solid #ddd; border-radius: 5px; }
            .section h2 { margin-top: 0; color: #0066cc; }
            button { padding: 10px 20px; margin: 5px; cursor: pointer; background: #0066cc; color: white; border: none; border-radius: 4px; }
            button:hover { background: #0052a3; }
            pre { background: #f4f4f4; padding: 15px; border-radius: 5px; overflow-x: auto; }
            .error-type { background: #fff3cd; padding: 10px; margin: 10px 0; border-left: 4px solid #ffc107; }
        </style>
    </head>
    <body>
        <h1>Error Handling & Problem Details Demo</h1>
        <p>This demo showcases comprehensive error handling using RFC 7807 Problem Details.</p>

        <div class="section">
            <h2>What is Problem Details?</h2>
            <p>Problem Details (RFC 7807) is a standard format for HTTP API error responses that provides:</p>
            <ul>
                <li><strong>type</strong> - URI reference identifying the problem type</li>
                <li><strong>title</strong> - Short, human-readable summary</li>
                <li><strong>status</strong> - HTTP status code</li>
                <li><strong>detail</strong> - Human-readable explanation</li>
                <li><strong>instance</strong> - URI reference identifying the specific occurrence</li>
            </ul>
        </div>

        <div class="section">
            <h2>Test Different Error Types</h2>
            
            <div class="error-type">
                <h3>400 Bad Request - Validation Error</h3>
                <button onclick="testError('/api/errors/validation')">Trigger Validation Error</button>
            </div>

            <div class="error-type">
                <h3>401 Unauthorized</h3>
                <button onclick="testError('/api/errors/unauthorized')">Trigger Unauthorized</button>
            </div>

            <div class="error-type">
                <h3>404 Not Found</h3>
                <button onclick="testError('/api/errors/notfound')">Trigger Not Found</button>
            </div>

            <div class="error-type">
                <h3>409 Conflict</h3>
                <button onclick="testError('/api/errors/conflict')">Trigger Conflict</button>
            </div>

            <div class="error-type">
                <h3>500 Internal Server Error</h3>
                <button onclick="testError('/api/errors/server')">Trigger Server Error</button>
            </div>

            <div class="error-type">
                <h3>Custom Business Exception</h3>
                <button onclick="testError('/api/errors/business')">Trigger Business Error</button>
            </div>
        </div>

        <div class="section">
            <h2>Response</h2>
            <pre id="response">Click a button above to see the error response...</pre>
        </div>

        <script>
            async function testError(endpoint) {
                try {
                    const response = await fetch(endpoint);
                    const data = await response.json();
                    
                    document.getElementById('response').textContent = JSON.stringify({
                        status: response.status,
                        statusText: response.statusText,
                        headers: {
                            'content-type': response.headers.get('content-type')
                        },
                        body: data
                    }, null, 2);
                } catch (error) {
                    document.getElementById('response').textContent = 'Error: ' + error.message;
                }
            }
        </script>
    </body>
    </html>
    """, "text/html"));

// Error demonstration endpoints

app.MapGet("/api/errors/validation", () =>
{
    throw new ValidationException("The 'email' field is required and must be a valid email address");
});

app.MapGet("/api/errors/unauthorized", () =>
{
    throw new UnauthorizedAccessException();
});

app.MapGet("/api/errors/notfound", () =>
{
    throw new KeyNotFoundException("User with ID 12345 was not found");
});

app.MapGet("/api/errors/conflict", () =>
{
    throw new InvalidOperationException("Cannot delete user because they have active orders");
});

app.MapGet("/api/errors/server", () =>
{
    throw new Exception("Database connection failed");
});

app.MapGet("/api/errors/business", () =>
{
    throw new BusinessException("Insufficient funds", "INSUFFICIENT_FUNDS", new { balance = 100, required = 250 });
});

// Endpoint that returns ProblemDetails directly
app.MapGet("/api/errors/direct", () =>
{
    return Results.Problem(
        title: "Direct Problem Details",
        detail: "This is a manually created ProblemDetails response",
        statusCode: StatusCodes.Status400BadRequest,
        type: "https://example.com/errors/custom"
    );
});

// Validation endpoint with model binding
app.MapPost("/api/users", ([FromBody] CreateUserRequest request) =>
{
    // Manual validation example
    var errors = new Dictionary<string, string[]>();

    if (string.IsNullOrWhiteSpace(request.Email))
        errors["email"] = new[] { "Email is required" };
    else if (!request.Email.Contains("@"))
        errors["email"] = new[] { "Email must be valid" };

    if (string.IsNullOrWhiteSpace(request.Name))
        errors["name"] = new[] { "Name is required" };

    if (request.Age < 18)
        errors["age"] = new[] { "Must be at least 18 years old" };

    if (errors.Any())
    {
        return Results.ValidationProblem(errors,
            title: "One or more validation errors occurred",
            detail: "Please check the errors property for details");
    }

    return Results.Ok(new { message = "User created successfully", user = request });
});

// Try-catch pattern example
app.MapGet("/api/safe/{id:int}", (int id) =>
{
    try
    {
        if (id <= 0)
        {
            return Results.Problem(
                title: "Invalid ID",
                detail: "ID must be greater than 0",
                statusCode: StatusCodes.Status400BadRequest
            );
        }

        if (id > 100)
        {
            return Results.Problem(
                title: "Not Found",
                detail: $"Resource with ID {id} was not found",
                statusCode: StatusCodes.Status404NotFound
            );
        }

        return Results.Ok(new { id, name = $"Resource {id}" });
    }
    catch (Exception ex)
    {
        return Results.Problem(
            title: "An error occurred",
            detail: ex.Message,
            statusCode: StatusCodes.Status500InternalServerError
        );
    }
});

app.MapControllers();

app.Run();

// Custom exception
public class BusinessException : Exception
{
    public string ErrorCode { get; }
    public object? AdditionalData { get; }

    public BusinessException(string message, string errorCode, object? additionalData = null)
        : base(message)
    {
        ErrorCode = errorCode;
        AdditionalData = additionalData;
    }
}

// Request model
public record CreateUserRequest(string Email, string Name, int Age);


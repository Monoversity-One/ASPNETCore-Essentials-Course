var builder = WebApplication.CreateBuilder(args);

// Add minimal services (Swagger for API exploration)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new() { Title = "Minimal API Basics", Version = "v1" });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ===== BASIC ENDPOINTS =====

// 1) Hello World - simplest endpoint
app.MapGet("/", () => Results.Text("Hello ASP.NET Core Minimal API! Visit /swagger to explore all endpoints.", "text/plain"))
    .WithName("Home")
    .WithTags("Basics");

// 2) Route parameters and typed results
app.MapGet("/greet/{name}", (string name) => TypedResults.Ok(new { message = $"Hello {name}!", timestamp = DateTime.UtcNow }))
    .WithName("Greet")
    .WithTags("Basics")
    .WithSummary("Greet a user by name")
    .WithDescription("Demonstrates route parameter binding and typed results");

// 3) Multiple route parameters
app.MapGet("/users/{userId:int}/posts/{postId:int}", (int userId, int postId) =>
    TypedResults.Ok(new { userId, postId, message = $"User {userId}, Post {postId}" }))
    .WithName("GetUserPost")
    .WithTags("Basics");

// 4) Query strings and validation
IResult SquareHandler(int? n)
{
    if (n is null) return TypedResults.BadRequest(new { error = "Missing query parameter 'n'" });
    return TypedResults.Ok(new { n, square = n * n, timestamp = DateTime.UtcNow });
}
app.MapGet("/square", SquareHandler)
    .WithName("Square")
    .WithTags("Basics");

// 5) Multiple query parameters
app.MapGet("/calculate", (int? a, int? b, string? operation) =>
{
    if (a is null || b is null || operation is null)
        return Results.BadRequest(new { error = "Parameters 'a', 'b', and 'operation' are required" });

    var result = operation.ToLower() switch
    {
        "add" => a + b,
        "subtract" => a - b,
        "multiply" => a * b,
        "divide" => b != 0 ? a / b : null,
        _ => null
    };

    if (result is null)
        return Results.BadRequest(new { error = "Invalid operation or division by zero" });

    return Results.Ok(new { a, b, operation, result });
})
    .WithName("Calculate")
    .WithTags("Basics");

// ===== REQUEST BINDING =====

// 6) FromBody binding with JSON
app.MapPost("/echo", (Echo payload) => TypedResults.Ok(new
{
    received = payload,
    timestamp = DateTime.UtcNow
}))
    .WithName("Echo")
    .WithTags("Binding")
    .WithSummary("Echo back the request body");

// 7) FromBody with validation
app.MapPost("/users", (CreateUserRequest request) =>
{
    // Validation can be added manually or with libraries like FluentValidation
    return TypedResults.Created($"/users/{Guid.NewGuid()}", new
    {
        id = Guid.NewGuid(),
        request.Name,
        request.Email,
        request.Age,
        createdAt = DateTime.UtcNow
    });
})
    .WithName("CreateUser")
    .WithTags("Binding");

// 8) Header binding (using HttpContext to access headers)
app.MapGet("/headers", (HttpContext context) =>
{
    var userAgent = context.Request.Headers["User-Agent"].ToString();
    var language = context.Request.Headers["Accept-Language"].ToString();
    return TypedResults.Ok(new { userAgent, language });
})
    .WithName("GetHeaders")
    .WithTags("Binding");

// 9) FromServices - dependency injection
app.MapGet("/time", (TimeProvider timeProvider) =>
    TypedResults.Ok(new { utcNow = timeProvider.GetUtcNow() }))
    .WithName("GetTime")
    .WithTags("Binding");

// ===== RESPONSE TYPES =====

// 10) Different response types
app.MapGet("/responses/{type}", (string type) =>
{
    return type.ToLower() switch
    {
        "ok" => Results.Ok(new { message = "Success", status = 200 }),
        "created" => Results.Created("/resource/123", new { id = 123 }),
        "accepted" => Results.Accepted("/status/456", new { taskId = 456 }),
        "nocontent" => Results.NoContent(),
        "badrequest" => Results.BadRequest(new { error = "Bad request example" }),
        "notfound" => Results.NotFound(new { error = "Resource not found" }),
        "unauthorized" => Results.Unauthorized(),
        "forbid" => Results.Forbid(),
        "json" => Results.Json(new { data = "JSON response" }),
        "text" => Results.Text("Plain text response"),
        "file" => Results.File(new byte[] { 1, 2, 3 }, "application/octet-stream", "sample.bin"),
        _ => Results.Problem(title: "Unknown type", statusCode: 400)
    };
})
    .WithName("ResponseTypes")
    .WithTags("Responses");

// 11) ProblemDetails for errors
app.MapGet("/problem", () => Results.Problem(
    title: "Demo error",
    detail: "This is a demonstration of RFC 7807 Problem Details",
    statusCode: 500,
    type: "https://example.com/errors/demo"))
    .WithName("ProblemDetails")
    .WithTags("Responses");

// 12) Redirect
app.MapGet("/redirect", () => Results.Redirect("/"))
    .WithName("Redirect")
    .WithTags("Responses");

// ===== ASYNC OPERATIONS =====

// 13) Async endpoint
app.MapGet("/async-data", async () =>
{
    await Task.Delay(100); // Simulate async work
    return TypedResults.Ok(new { data = "Async result", timestamp = DateTime.UtcNow });
})
    .WithName("AsyncData")
    .WithTags("Async");

// 14) Async with cancellation token
app.MapGet("/long-running", async (CancellationToken cancellationToken) =>
{
    try
    {
        await Task.Delay(5000, cancellationToken);
        return Results.Ok(new { message = "Completed" });
    }
    catch (OperationCanceledException)
    {
        return Results.StatusCode(499); // Client closed request
    }
})
    .WithName("LongRunning")
    .WithTags("Async");

// ===== HTTP METHODS =====

// 15) All HTTP methods
var todos = new List<Todo>();

app.MapGet("/todos", () => TypedResults.Ok(todos))
    .WithName("GetTodos")
    .WithTags("CRUD");

IResult GetTodoById(int id)
{
    var todo = todos.FirstOrDefault(t => t.Id == id);
    if (todo is null)
    {
        return TypedResults.NotFound();
    }
    return TypedResults.Ok(todo);
}

app.MapGet("/todos/{id}", GetTodoById)
    .WithName("GetTodo")
    .WithTags("CRUD");

app.MapPost("/todos", (CreateTodoRequest request) =>
{
    var todo = new Todo(todos.Count + 1, request.Title, false);
    todos.Add(todo);
    return TypedResults.Created($"/todos/{todo.Id}", todo);
})
    .WithName("CreateTodo")
    .WithTags("CRUD");

app.MapPut("/todos/{id:int}", (int id, UpdateTodoRequest request) =>
{
    var todo = todos.FirstOrDefault(t => t.Id == id);
    if (todo is null) return Results.NotFound();

    var updated = todo with { Title = request.Title, IsCompleted = request.IsCompleted };
    todos[todos.IndexOf(todo)] = updated;
    return TypedResults.Ok(updated);
})
    .WithName("UpdateTodo")
    .WithTags("CRUD");

app.MapDelete("/todos/{id:int}", (int id) =>
{
    var removed = todos.RemoveAll(t => t.Id == id) > 0;
    return removed ? Results.NoContent() : Results.NotFound();
})
    .WithName("DeleteTodo")
    .WithTags("CRUD");

app.Run();

// ===== MODELS =====

public record Echo(string? Message, int Count);

public record CreateUserRequest(string Email, string Name, int Age);

public record Todo(int Id, string Title, bool IsCompleted);
public record CreateTodoRequest(string Title);
public record UpdateTodoRequest(string Title, bool IsCompleted);

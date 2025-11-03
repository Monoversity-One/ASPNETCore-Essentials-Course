using GrpcServices.Services;

var builder = WebApplication.CreateBuilder(args);

// Add gRPC services
builder.Services.AddGrpc(options =>
{
    options.EnableDetailedErrors = true;
    options.MaxReceiveMessageSize = 2 * 1024 * 1024; // 2 MB
    options.MaxSendMessageSize = 5 * 1024 * 1024; // 5 MB
});

// Add gRPC reflection for tools like grpcurl
builder.Services.AddGrpcReflection();

// Add product repository
builder.Services.AddSingleton<ProductRepository>();

var app = builder.Build();

// Map gRPC services
app.MapGrpcService<GreeterService>();
app.MapGrpcService<ProductServiceImpl>();

// Enable gRPC reflection in development
if (app.Environment.IsDevelopment())
{
    app.MapGrpcReflectionService();
}

// Provide instructions on the default endpoint
app.MapGet("/", () => Results.Content("""
    <!DOCTYPE html>
    <html>
    <head>
        <title>gRPC Services Demo</title>
        <style>
            body { font-family: Arial, sans-serif; max-width: 900px; margin: 50px auto; padding: 20px; }
            pre { background: #f4f4f4; padding: 15px; border-radius: 5px; overflow-x: auto; }
            code { background: #f4f4f4; padding: 2px 6px; border-radius: 3px; }
            .section { margin: 30px 0; }
        </style>
    </head>
    <body>
        <h1>gRPC Services Demo</h1>
        <p>This application demonstrates gRPC services in ASP.NET Core.</p>
        
        <div class="section">
            <h2>Available Services</h2>
            <ul>
                <li><strong>Greeter Service</strong> - Demonstrates all 4 types of gRPC calls</li>
                <li><strong>Product Service</strong> - CRUD operations with gRPC</li>
            </ul>
        </div>

        <div class="section">
            <h2>Testing with grpcurl</h2>
            <p>Install grpcurl: <code>go install github.com/fullstorydev/grpcurl/cmd/grpcurl@latest</code></p>
            
            <h3>List available services:</h3>
            <pre>grpcurl -plaintext localhost:51152 list</pre>
            
            <h3>Describe a service:</h3>
            <pre>grpcurl -plaintext localhost:51152 describe greet.Greeter</pre>
            
            <h3>Call SayHello (Unary):</h3>
            <pre>grpcurl -plaintext -d '{"name": "World"}' localhost:51152 greet.Greeter/SayHello</pre>
            
            <h3>Call SayHellos (Server Streaming):</h3>
            <pre>grpcurl -plaintext -d '{"name": "World"}' localhost:51152 greet.Greeter/SayHellos</pre>
            
            <h3>List Products:</h3>
            <pre>grpcurl -plaintext -d '{"page_size": 10, "page_number": 1}' localhost:51152 products.ProductService/ListProducts</pre>
            
            <h3>Create Product:</h3>
            <pre>grpcurl -plaintext -d '{"name": "Laptop", "description": "Gaming laptop", "price": 1299.99, "stock": 10}' localhost:51152 products.ProductService/CreateProduct</pre>
        </div>

        <div class="section">
            <h2>Testing with .NET Client</h2>
            <p>Add NuGet packages:</p>
            <pre>dotnet add package Grpc.Net.Client
dotnet add package Google.Protobuf
dotnet add package Grpc.Tools</pre>
            
            <p>Sample C# client code:</p>
            <pre>using Grpc.Net.Client;
using GrpcServices;

var channel = GrpcChannel.ForAddress("https://localhost:51150");
var client = new Greeter.GreeterClient(channel);

var reply = await client.SayHelloAsync(new HelloRequest { Name = "World" });
Console.WriteLine(reply.Message);</pre>
        </div>

        <div class="section">
            <h2>Proto Files</h2>
            <p>Proto definitions are in the <code>Protos/</code> folder:</p>
            <ul>
                <li><code>greeter.proto</code> - Greeting service with all RPC types</li>
                <li><code>products.proto</code> - Product CRUD service</li>
            </ul>
        </div>
    </body>
    </html>
""", "text/html"));

app.Run();


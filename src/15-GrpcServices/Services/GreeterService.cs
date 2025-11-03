using Grpc.Core;

namespace GrpcServices.Services;

/// <summary>
/// Demonstrates all four types of gRPC calls:
/// 1. Unary RPC
/// 2. Server streaming RPC
/// 3. Client streaming RPC
/// 4. Bidirectional streaming RPC
/// </summary>
public class GreeterService : Greeter.GreeterBase
{
    private readonly ILogger<GreeterService> _logger;

    public GreeterService(ILogger<GreeterService> logger)
    {
        _logger = logger;
    }

    // 1. Unary RPC - single request, single response
    public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
    {
        _logger.LogInformation("Received SayHello request for {Name}", request.Name);
        
        return Task.FromResult(new HelloReply
        {
            Message = $"Hello {request.Name}!",
            Timestamp = DateTime.UtcNow.ToString("O")
        });
    }

    // 2. Server streaming RPC - single request, stream of responses
    public override async Task SayHellos(HelloRequest request, IServerStreamWriter<HelloReply> responseStream, ServerCallContext context)
    {
        _logger.LogInformation("Received SayHellos request for {Name}", request.Name);

        for (int i = 1; i <= 5; i++)
        {
            // Check if client cancelled the request
            if (context.CancellationToken.IsCancellationRequested)
            {
                _logger.LogInformation("Client cancelled the request");
                break;
            }

            await responseStream.WriteAsync(new HelloReply
            {
                Message = $"Hello {request.Name} #{i}!",
                Timestamp = DateTime.UtcNow.ToString("O")
            });

            await Task.Delay(500); // Simulate some work
        }
    }

    // 3. Client streaming RPC - stream of requests, single response
    public override async Task<HelloSummary> SayHelloToMany(IAsyncStreamReader<HelloRequest> requestStream, ServerCallContext context)
    {
        _logger.LogInformation("Received SayHelloToMany request");

        var names = new List<string>();

        await foreach (var request in requestStream.ReadAllAsync())
        {
            names.Add(request.Name);
            _logger.LogInformation("Received name: {Name}", request.Name);
        }

        return new HelloSummary
        {
            Count = names.Count,
            Message = $"Received greetings from: {string.Join(", ", names)}"
        };
    }

    // 4. Bidirectional streaming RPC
    public override async Task Chat(IAsyncStreamReader<ChatMessage> requestStream, IServerStreamWriter<ChatMessage> responseStream, ServerCallContext context)
    {
        _logger.LogInformation("Chat session started");

        // Send welcome message
        await responseStream.WriteAsync(new ChatMessage
        {
            User = "Server",
            Message = "Welcome to the chat! Type your messages.",
            Timestamp = DateTime.UtcNow.ToString("O")
        });

        // Echo back messages with some processing
        await foreach (var message in requestStream.ReadAllAsync())
        {
            _logger.LogInformation("Chat message from {User}: {Message}", message.User, message.Message);

            // Echo the message back
            await responseStream.WriteAsync(new ChatMessage
            {
                User = "Server",
                Message = $"Echo: {message.Message}",
                Timestamp = DateTime.UtcNow.ToString("O")
            });

            // Send a response based on content
            if (message.Message.Contains("hello", StringComparison.OrdinalIgnoreCase))
            {
                await responseStream.WriteAsync(new ChatMessage
                {
                    User = "Server",
                    Message = $"Hello {message.User}! How can I help you?",
                    Timestamp = DateTime.UtcNow.ToString("O")
                });
            }
        }

        _logger.LogInformation("Chat session ended");
    }
}


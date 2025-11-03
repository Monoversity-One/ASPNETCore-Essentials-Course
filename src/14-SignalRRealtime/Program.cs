using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

var builder = WebApplication.CreateBuilder(args);

// Add SignalR services
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true;
    options.KeepAliveInterval = TimeSpan.FromSeconds(10);
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
});

// Add CORS for SignalR client testing
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Singleton service to manage chat state
builder.Services.AddSingleton<ChatRoomService>();

var app = builder.Build();

app.UseCors();
app.UseStaticFiles();

// Map SignalR hubs
app.MapHub<ChatHub>("/hubs/chat");
app.MapHub<NotificationHub>("/hubs/notifications");
app.MapHub<StockTickerHub>("/hubs/stockticker");

// REST API endpoints to trigger SignalR notifications
app.MapGet("/", () => Results.Content("""
    <!DOCTYPE html>
    <html>
    <head>
        <title>SignalR Demo</title>
        <script src="https://cdn.jsdelivr.net/npm/@microsoft/signalr@8.0.0/dist/browser/signalr.min.js"></script>
        <style>
            body { font-family: Arial, sans-serif; max-width: 800px; margin: 50px auto; padding: 20px; }
            .section { margin: 20px 0; padding: 15px; border: 1px solid #ddd; border-radius: 5px; }
            input, button { padding: 8px; margin: 5px; }
            #messages { height: 200px; overflow-y: auto; border: 1px solid #ccc; padding: 10px; margin: 10px 0; }
            .message { margin: 5px 0; }
        </style>
    </head>
    <body>
        <h1>SignalR Real-time Communication Demo</h1>
        
        <div class="section">
            <h2>Chat Room</h2>
            <div>
                <input type="text" id="username" placeholder="Your name" value="User1" />
                <button onclick="connectChat()">Connect</button>
                <button onclick="disconnectChat()">Disconnect</button>
            </div>
            <div id="messages"></div>
            <div>
                <input type="text" id="messageInput" placeholder="Type a message..." style="width: 60%;" />
                <button onclick="sendMessage()">Send</button>
            </div>
        </div>

        <div class="section">
            <h2>Notifications</h2>
            <button onclick="connectNotifications()">Connect to Notifications</button>
            <button onclick="triggerNotification()">Trigger Server Notification</button>
            <div id="notifications" style="margin-top: 10px;"></div>
        </div>

        <div class="section">
            <h2>Stock Ticker (Streaming)</h2>
            <button onclick="connectStockTicker()">Start Streaming</button>
            <button onclick="disconnectStockTicker()">Stop Streaming</button>
            <div id="stocks" style="margin-top: 10px;"></div>
        </div>

        <script>
            let chatConnection = null;
            let notificationConnection = null;
            let stockConnection = null;

            // Chat Hub
            async function connectChat() {
                const username = document.getElementById('username').value;
                chatConnection = new signalR.HubConnectionBuilder()
                    .withUrl("/hubs/chat")
                    .withAutomaticReconnect()
                    .build();

                chatConnection.on("ReceiveMessage", (user, message, timestamp) => {
                    const div = document.createElement("div");
                    div.className = "message";
                    div.innerHTML = `<strong>${user}</strong> (${new Date(timestamp).toLocaleTimeString()}): ${message}`;
                    document.getElementById("messages").appendChild(div);
                });

                chatConnection.on("UserJoined", (user) => {
                    const div = document.createElement("div");
                    div.className = "message";
                    div.innerHTML = `<em>${user} joined the chat</em>`;
                    document.getElementById("messages").appendChild(div);
                });

                await chatConnection.start();
                await chatConnection.invoke("JoinChat", username);
            }

            async function disconnectChat() {
                if (chatConnection) {
                    await chatConnection.stop();
                }
            }

            async function sendMessage() {
                const message = document.getElementById('messageInput').value;
                if (chatConnection && message) {
                    await chatConnection.invoke("SendMessage", message);
                    document.getElementById('messageInput').value = '';
                }
            }

            // Notification Hub
            async function connectNotifications() {
                notificationConnection = new signalR.HubConnectionBuilder()
                    .withUrl("/hubs/notifications")
                    .build();

                notificationConnection.on("ReceiveNotification", (title, message, severity) => {
                    const div = document.createElement("div");
                    div.style.padding = "10px";
                    div.style.margin = "5px 0";
                    div.style.backgroundColor = severity === "Error" ? "#ffcccc" : "#ccffcc";
                    div.innerHTML = `<strong>${title}</strong>: ${message}`;
                    document.getElementById("notifications").insertBefore(div, document.getElementById("notifications").firstChild);
                });

                await notificationConnection.start();
                alert("Connected to notifications!");
            }

            async function triggerNotification() {
                await fetch('/api/notify', { method: 'POST' });
            }

            // Stock Ticker Hub
            async function connectStockTicker() {
                stockConnection = new signalR.HubConnectionBuilder()
                    .withUrl("/hubs/stockticker")
                    .build();

                stockConnection.on("UpdateStock", (symbol, price, change) => {
                    const color = change >= 0 ? "green" : "red";
                    const existing = document.getElementById(`stock-${symbol}`);
                    const html = `<span style="color: ${color}">${symbol}: $${price.toFixed(2)} (${change >= 0 ? '+' : ''}${change.toFixed(2)}%)</span>`;
                    
                    if (existing) {
                        existing.innerHTML = html;
                    } else {
                        const div = document.createElement("div");
                        div.id = `stock-${symbol}`;
                        div.innerHTML = html;
                        document.getElementById("stocks").appendChild(div);
                    }
                });

                await stockConnection.start();
                await stockConnection.invoke("StartStreaming");
            }

            async function disconnectStockTicker() {
                if (stockConnection) {
                    await stockConnection.invoke("StopStreaming");
                    await stockConnection.stop();
                }
            }
        </script>
    </body>
    </html>
    """, "text/html"));

app.MapPost("/api/notify", async (IHubContext<NotificationHub> hubContext) =>
{
    await hubContext.Clients.All.SendAsync("ReceiveNotification",
        "Server Alert",
        $"Notification sent at {DateTime.Now:HH:mm:ss}",
        "Info");
    return Results.Ok(new { sent = true });
});

app.MapGet("/api/chat/users", (ChatRoomService chatService) =>
    Results.Ok(chatService.GetActiveUsers()));

app.Run();

// Chat Hub - demonstrates group messaging and user management
public class ChatHub : Hub
{
    private readonly ChatRoomService _chatService;

    public ChatHub(ChatRoomService chatService)
    {
        _chatService = chatService;
    }

    public async Task JoinChat(string username)
    {
        _chatService.AddUser(Context.ConnectionId, username);
        await Clients.Others.SendAsync("UserJoined", username);
        await Clients.Caller.SendAsync("ReceiveMessage", "System", $"Welcome {username}!", DateTime.UtcNow);
    }

    public async Task SendMessage(string message)
    {
        var username = _chatService.GetUsername(Context.ConnectionId);
        if (username != null)
        {
            await Clients.All.SendAsync("ReceiveMessage", username, message, DateTime.UtcNow);
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var username = _chatService.RemoveUser(Context.ConnectionId);
        if (username != null)
        {
            await Clients.Others.SendAsync("UserLeft", username);
        }
        await base.OnDisconnectedAsync(exception);
    }
}

// Notification Hub - demonstrates server-to-client push notifications
public class NotificationHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        await Clients.Caller.SendAsync("ReceiveNotification", 
            "Connected", 
            "You are now connected to notifications", 
            "Success");
        await base.OnConnectedAsync();
    }
}

// Stock Ticker Hub - demonstrates streaming data
public class StockTickerHub : Hub
{
    private static readonly string[] Symbols = { "AAPL", "GOOGL", "MSFT", "AMZN", "TSLA" };
    private readonly Dictionary<string, decimal> _prices = new();
    private CancellationTokenSource? _cts;

    public async Task StartStreaming()
    {
        _cts = new CancellationTokenSource();
        
        // Initialize prices
        foreach (var symbol in Symbols)
        {
            _prices[symbol] = Random.Shared.Next(100, 500);
        }

        try
        {
            while (!_cts.Token.IsCancellationRequested)
            {
                foreach (var symbol in Symbols)
                {
                    var oldPrice = _prices[symbol];
                    var change = (decimal)(Random.Shared.NextDouble() * 4 - 2); // -2% to +2%
                    var newPrice = oldPrice * (1 + change / 100);
                    _prices[symbol] = newPrice;

                    await Clients.Caller.SendAsync("UpdateStock", symbol, newPrice, change);
                }

                await Task.Delay(1000, _cts.Token);
            }
        }
        catch (OperationCanceledException)
        {
            // Expected when stopping
        }
    }

    public Task StopStreaming()
    {
        _cts?.Cancel();
        return Task.CompletedTask;
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        _cts?.Cancel();
        return base.OnDisconnectedAsync(exception);
    }
}

// Service to manage chat room state
public class ChatRoomService
{
    private readonly ConcurrentDictionary<string, string> _users = new();

    public void AddUser(string connectionId, string username)
    {
        _users[connectionId] = username;
    }

    public string? RemoveUser(string connectionId)
    {
        _users.TryRemove(connectionId, out var username);
        return username;
    }

    public string? GetUsername(string connectionId)
    {
        _users.TryGetValue(connectionId, out var username);
        return username;
    }

    public IEnumerable<string> GetActiveUsers()
    {
        return _users.Values;
    }
}


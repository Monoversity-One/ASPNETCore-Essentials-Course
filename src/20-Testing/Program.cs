using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    ContentRootPath = AppContext.BaseDirectory
});

builder.Services.AddControllers();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddSingleton<IProductRepository, InMemoryProductRepository>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

// Minimal API endpoints for testing
app.MapGet("/api/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }));

app.MapGet("/api/echo/{message}", (string message) => Results.Ok(new { echo = message }));

app.MapPost("/api/calculate", (CalculationRequest request) =>
{
    var result = request.Operation switch
    {
        "add" => request.A + request.B,
        "subtract" => request.A - request.B,
        "multiply" => request.A * request.B,
        "divide" => request.B != 0 ? request.A / request.B : throw new DivideByZeroException(),
        _ => throw new ArgumentException("Invalid operation")
    };

    return Results.Ok(new { result });
});

app.MapGet("/", () => Results.Content("""
    <!DOCTYPE html>
    <html>
    <head>
        <title>Testing Demo</title>
        <style>
            body { font-family: Arial, sans-serif; max-width: 1000px; margin: 50px auto; padding: 20px; }
            .section { margin: 30px 0; padding: 20px; border: 1px solid #ddd; border-radius: 5px; }
            h2 { color: #0066cc; }
            pre { background: #f4f4f4; padding: 15px; border-radius: 5px; overflow-x: auto; }
            code { background: #f4f4f4; padding: 2px 6px; border-radius: 3px; }
        </style>
    </head>
    <body>
        <h1>Testing Demo Application</h1>
        <p>This application demonstrates testable ASP.NET Core code with unit and integration tests.</p>

        <div class="section">
            <h2>Available Endpoints</h2>
            <ul>
                <li><code>GET /api/health</code> - Health check</li>
                <li><code>GET /api/echo/{message}</code> - Echo message</li>
                <li><code>POST /api/calculate</code> - Calculate operations</li>
                <li><code>GET /api/products</code> - List products</li>
                <li><code>GET /api/products/{id}</code> - Get product by ID</li>
                <li><code>POST /api/products</code> - Create product</li>
                <li><code>PUT /api/products/{id}</code> - Update product</li>
                <li><code>DELETE /api/products/{id}</code> - Delete product</li>
            </ul>
        </div>

        <div class="section">
            <h2>Running Tests</h2>
            <p>This project includes comprehensive tests. To run them:</p>
            <pre>cd src/20-Testing
    dotnet test</pre>

            <h3>Test Coverage</h3>
            <ul>
                <li><strong>Unit Tests</strong> - Test business logic in isolation</li>
                <li><strong>Integration Tests</strong> - Test full HTTP request/response cycle</li>
                <li><strong>Repository Tests</strong> - Test data access layer</li>
                <li><strong>Service Tests</strong> - Test service layer with mocks</li>
            </ul>
        </div>

        <div class="section">
            <h2>Testing Best Practices Demonstrated</h2>
            <ul>
                <li>Dependency Injection for testability</li>
                <li>Interface-based design</li>
                <li>WebApplicationFactory for integration tests</li>
                <li>Arrange-Act-Assert pattern</li>
                <li>Test data builders</li>
                <li>Mocking with NSubstitute</li>
            </ul>
        </div>

        <div class="section">
            <h2>Swagger Documentation</h2>
            <p>Visit <a href="/swagger">/swagger</a> to see the API documentation and test endpoints interactively.</p>
        </div>
    </body>
    </html>
    """, "text/html"));

app.Run();

// Make the Program class accessible to tests
public partial class Program { }

// Models
public record Product(int Id, string Name, string Description, decimal Price, int Stock);
public record CreateProductDto(string Name, string Description, decimal Price, int Stock);
public record UpdateProductDto(string Name, string Description, decimal Price, int Stock);
public record CalculationRequest(double A, double B, string Operation);

// Interfaces
public interface IProductRepository
{
    Task<IEnumerable<Product>> GetAllAsync();
    Task<Product?> GetByIdAsync(int id);
    Task<Product> CreateAsync(Product product);
    Task<Product?> UpdateAsync(int id, Product product);
    Task<bool> DeleteAsync(int id);
}

public interface IProductService
{
    Task<IEnumerable<Product>> GetAllProductsAsync();
    Task<Product?> GetProductByIdAsync(int id);
    Task<Product> CreateProductAsync(CreateProductDto dto);
    Task<Product?> UpdateProductAsync(int id, UpdateProductDto dto);
    Task<bool> DeleteProductAsync(int id);
}

// Repository implementation
public class InMemoryProductRepository : IProductRepository
{
    private readonly List<Product> _products = new();
    private int _nextId = 1;

    public InMemoryProductRepository()
    {
        // Seed data
        _products.Add(new Product(GetNextId(), "Laptop", "High-performance laptop", 1299.99m, 10));
        _products.Add(new Product(GetNextId(), "Mouse", "Wireless mouse", 29.99m, 50));
        _products.Add(new Product(GetNextId(), "Keyboard", "Mechanical keyboard", 89.99m, 30));
    }

    private int GetNextId() => _nextId++;

    public Task<IEnumerable<Product>> GetAllAsync()
    {
        return Task.FromResult<IEnumerable<Product>>(_products);
    }

    public Task<Product?> GetByIdAsync(int id)
    {
        var product = _products.FirstOrDefault(p => p.Id == id);
        return Task.FromResult(product);
    }

    public Task<Product> CreateAsync(Product product)
    {
        var newProduct = product with { Id = GetNextId() };
        _products.Add(newProduct);
        return Task.FromResult(newProduct);
    }

    public Task<Product?> UpdateAsync(int id, Product product)
    {
        var index = _products.FindIndex(p => p.Id == id);
        if (index == -1) return Task.FromResult<Product?>(null);

        var updated = product with { Id = id };
        _products[index] = updated;
        return Task.FromResult<Product?>(updated);
    }

    public Task<bool> DeleteAsync(int id)
    {
        var removed = _products.RemoveAll(p => p.Id == id) > 0;
        return Task.FromResult(removed);
    }
}

// Service implementation
public class ProductService : IProductService
{
    private readonly IProductRepository _repository;

    public ProductService(IProductRepository repository)
    {
        _repository = repository;
    }

    public Task<IEnumerable<Product>> GetAllProductsAsync()
    {
        return _repository.GetAllAsync();
    }

    public Task<Product?> GetProductByIdAsync(int id)
    {
        if (id <= 0)
            throw new ArgumentException("ID must be greater than 0", nameof(id));

        return _repository.GetByIdAsync(id);
    }

    public Task<Product> CreateProductAsync(CreateProductDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new ArgumentException("Name is required", nameof(dto.Name));

        if (dto.Price <= 0)
            throw new ArgumentException("Price must be greater than 0", nameof(dto.Price));

        var product = new Product(0, dto.Name, dto.Description, dto.Price, dto.Stock);
        return _repository.CreateAsync(product);
    }

    public async Task<Product?> UpdateProductAsync(int id, UpdateProductDto dto)
    {
        if (id <= 0)
            throw new ArgumentException("ID must be greater than 0", nameof(id));

        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new ArgumentException("Name is required", nameof(dto.Name));

        var existing = await _repository.GetByIdAsync(id);
        if (existing == null)
            return null;

        var updated = new Product(id, dto.Name, dto.Description, dto.Price, dto.Stock);
        return await _repository.UpdateAsync(id, updated);
    }

    public Task<bool> DeleteProductAsync(int id)
    {
        if (id <= 0)
            throw new ArgumentException("ID must be greater than 0", nameof(id));

        return _repository.DeleteAsync(id);
    }
}

// Controller
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _service;

    public ProductsController(IProductService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Product>>> GetAll()
    {
        var products = await _service.GetAllProductsAsync();
        return Ok(products);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Product>> GetById(int id)
    {
        try
        {
            var product = await _service.GetProductByIdAsync(id);
            if (product == null)
                return NotFound();

            return Ok(product);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost]
    public async Task<ActionResult<Product>> Create(CreateProductDto dto)
    {
        try
        {
            var product = await _service.CreateProductAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<Product>> Update(int id, UpdateProductDto dto)
    {
        try
        {
            var product = await _service.UpdateProductAsync(id, dto);
            if (product == null)
                return NotFound();

            return Ok(product);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        try
        {
            var deleted = await _service.DeleteProductAsync(id);
            if (!deleted)
                return NotFound();

            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}


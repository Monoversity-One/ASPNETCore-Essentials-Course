using Grpc.Core;
using System.Collections.Concurrent;

namespace GrpcServices.Services;

/// <summary>
/// Demonstrates CRUD operations with gRPC
/// </summary>
public class ProductServiceImpl : ProductService.ProductServiceBase
{
    private readonly ILogger<ProductServiceImpl> _logger;
    private readonly ProductRepository _repository;

    public ProductServiceImpl(ILogger<ProductServiceImpl> logger, ProductRepository repository)
    {
        _logger = logger;
        _repository = repository;
    }

    public override Task<Product> GetProduct(GetProductRequest request, ServerCallContext context)
    {
        _logger.LogInformation("GetProduct called for ID: {Id}", request.Id);

        var product = _repository.GetById(request.Id);
        if (product == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, $"Product with ID {request.Id} not found"));
        }

        return Task.FromResult(product);
    }

    public override Task<ListProductsResponse> ListProducts(ListProductsRequest request, ServerCallContext context)
    {
        _logger.LogInformation("ListProducts called - Page: {Page}, Size: {Size}", request.PageNumber, request.PageSize);

        var allProducts = _repository.GetAll();
        var pageSize = request.PageSize > 0 ? request.PageSize : 10;
        var pageNumber = request.PageNumber > 0 ? request.PageNumber : 1;

        var products = allProducts
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var response = new ListProductsResponse
        {
            TotalCount = allProducts.Count
        };
        response.Products.AddRange(products);

        return Task.FromResult(response);
    }

    public override Task<Product> CreateProduct(CreateProductRequest request, ServerCallContext context)
    {
        _logger.LogInformation("CreateProduct called: {Name}", request.Name);

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Product name is required"));
        }

        if (request.Price <= 0)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Price must be greater than 0"));
        }

        var product = new Product
        {
            Id = _repository.GetNextId(),
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            Stock = request.Stock
        };

        _repository.Add(product);

        return Task.FromResult(product);
    }

    public override Task<Product> UpdateProduct(UpdateProductRequest request, ServerCallContext context)
    {
        _logger.LogInformation("UpdateProduct called for ID: {Id}", request.Id);

        var existing = _repository.GetById(request.Id);
        if (existing == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, $"Product with ID {request.Id} not found"));
        }

        var updated = new Product
        {
            Id = request.Id,
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            Stock = request.Stock
        };

        _repository.Update(updated);

        return Task.FromResult(updated);
    }

    public override Task<DeleteProductResponse> DeleteProduct(DeleteProductRequest request, ServerCallContext context)
    {
        _logger.LogInformation("DeleteProduct called for ID: {Id}", request.Id);

        var success = _repository.Delete(request.Id);

        if (!success)
        {
            throw new RpcException(new Status(StatusCode.NotFound, $"Product with ID {request.Id} not found"));
        }

        return Task.FromResult(new DeleteProductResponse
        {
            Success = true,
            Message = $"Product {request.Id} deleted successfully"
        });
    }
}

/// <summary>
/// In-memory product repository for demo purposes
/// </summary>
public class ProductRepository
{
    private readonly ConcurrentDictionary<int, Product> _products = new();
    private int _nextId = 1;

    public ProductRepository()
    {
        // Seed some initial data
        Add(new Product { Id = GetNextId(), Name = "Laptop", Description = "High-performance laptop", Price = 1299.99, Stock = 15 });
        Add(new Product { Id = GetNextId(), Name = "Mouse", Description = "Wireless mouse", Price = 29.99, Stock = 50 });
        Add(new Product { Id = GetNextId(), Name = "Keyboard", Description = "Mechanical keyboard", Price = 89.99, Stock = 30 });
    }

    public int GetNextId() => Interlocked.Increment(ref _nextId);

    public Product? GetById(int id)
    {
        _products.TryGetValue(id, out var product);
        return product;
    }

    public List<Product> GetAll()
    {
        return _products.Values.ToList();
    }

    public void Add(Product product)
    {
        _products[product.Id] = product;
    }

    public void Update(Product product)
    {
        _products[product.Id] = product;
    }

    public bool Delete(int id)
    {
        return _products.TryRemove(id, out _);
    }
}


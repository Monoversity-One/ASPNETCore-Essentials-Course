using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Testing.Tests.IntegrationTests;

/// <summary>
/// Integration tests using WebApplicationFactory
/// Tests the full HTTP request/response cycle
/// </summary>
public class ProductsApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public ProductsApiTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAllProducts_ReturnsSuccessAndProducts()
    {
        // Act
        var response = await _client.GetAsync("/api/products");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var products = await response.Content.ReadFromJsonAsync<List<Product>>();
        products.Should().NotBeNull();
        products.Should().NotBeEmpty();
        products.Should().HaveCountGreaterThan(0);
    }

    [Fact]
    public async Task GetProductById_WithValidId_ReturnsProduct()
    {
        // Arrange
        var productId = 1;

        // Act
        var response = await _client.GetAsync($"/api/products/{productId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var product = await response.Content.ReadFromJsonAsync<Product>();
        product.Should().NotBeNull();
        product!.Id.Should().Be(productId);
    }

    [Fact]
    public async Task GetProductById_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var productId = 9999;

        // Act
        var response = await _client.GetAsync($"/api/products/{productId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetProductById_WithZeroId_ReturnsBadRequest()
    {
        // Act
        var response = await _client.GetAsync("/api/products/0");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateProduct_WithValidData_ReturnsCreated()
    {
        // Arrange
        var newProduct = new CreateProductDto(
            Name: "Test Product",
            Description: "Test Description",
            Price: 99.99m,
            Stock: 10
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/products", newProduct);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
        
        var product = await response.Content.ReadFromJsonAsync<Product>();
        product.Should().NotBeNull();
        product!.Name.Should().Be(newProduct.Name);
        product.Price.Should().Be(newProduct.Price);
    }

    [Fact]
    public async Task CreateProduct_WithInvalidData_ReturnsBadRequest()
    {
        // Arrange
        var invalidProduct = new CreateProductDto(
            Name: "",
            Description: "Test",
            Price: -10,
            Stock: 5
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/products", invalidProduct);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateProduct_WithValidData_ReturnsOk()
    {
        // Arrange
        var productId = 1;
        var updateDto = new UpdateProductDto(
            Name: "Updated Product",
            Description: "Updated Description",
            Price: 199.99m,
            Stock: 20
        );

        // Act
        var response = await _client.PutAsJsonAsync($"/api/products/{productId}", updateDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var product = await response.Content.ReadFromJsonAsync<Product>();
        product.Should().NotBeNull();
        product!.Name.Should().Be(updateDto.Name);
    }

    [Fact]
    public async Task DeleteProduct_WithValidId_ReturnsNoContent()
    {
        // Arrange - First create a product to delete
        var newProduct = new CreateProductDto("To Delete", "Will be deleted", 50m, 5);
        var createResponse = await _client.PostAsJsonAsync("/api/products", newProduct);
        var created = await createResponse.Content.ReadFromJsonAsync<Product>();

        // Act
        var response = await _client.DeleteAsync($"/api/products/{created!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify it's actually deleted
        var getResponse = await _client.GetAsync($"/api/products/{created.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task HealthEndpoint_ReturnsHealthy()
    {
        // Act
        var response = await _client.GetAsync("/api/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var health = await response.Content.ReadFromJsonAsync<HealthResponse>();
        health.Should().NotBeNull();
        health!.Status.Should().Be("healthy");
    }

    [Theory]
    [InlineData("add", 5, 3, 8)]
    [InlineData("subtract", 10, 4, 6)]
    [InlineData("multiply", 6, 7, 42)]
    [InlineData("divide", 20, 4, 5)]
    public async Task Calculate_WithValidOperation_ReturnsCorrectResult(
        string operation, double a, double b, double expected)
    {
        // Arrange
        var request = new CalculationRequest(a, b, operation);

        // Act
        var response = await _client.PostAsJsonAsync("/api/calculate", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<CalculationResponse>();
        result.Should().NotBeNull();
        result!.Result.Should().Be(expected);
    }

    private record HealthResponse(string Status, DateTime Timestamp);
    private record CalculationResponse(double Result);
}


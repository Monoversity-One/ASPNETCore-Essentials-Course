using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Testing.Tests.UnitTests;

/// <summary>
/// Unit tests for ProductService using mocks
/// Tests business logic in isolation
/// </summary>
public class ProductServiceTests
{
    private readonly IProductRepository _mockRepository;
    private readonly ProductService _service;

    public ProductServiceTests()
    {
        _mockRepository = Substitute.For<IProductRepository>();
        _service = new ProductService(_mockRepository);
    }

    [Fact]
    public async Task GetAllProductsAsync_ReturnsAllProducts()
    {
        // Arrange
        var expectedProducts = new List<Product>
        {
            new(1, "Product 1", "Desc 1", 10m, 5),
            new(2, "Product 2", "Desc 2", 20m, 10)
        };
        _mockRepository.GetAllAsync().Returns(expectedProducts);

        // Act
        var result = await _service.GetAllProductsAsync();

        // Assert
        result.Should().BeEquivalentTo(expectedProducts);
        await _mockRepository.Received(1).GetAllAsync();
    }

    [Fact]
    public async Task GetProductByIdAsync_WithValidId_ReturnsProduct()
    {
        // Arrange
        var expectedProduct = new Product(1, "Test", "Description", 99.99m, 10);
        _mockRepository.GetByIdAsync(1).Returns(expectedProduct);

        // Act
        var result = await _service.GetProductByIdAsync(1);

        // Assert
        result.Should().BeEquivalentTo(expectedProduct);
        await _mockRepository.Received(1).GetByIdAsync(1);
    }

    [Fact]
    public async Task GetProductByIdAsync_WithInvalidId_ThrowsArgumentException()
    {
        // Act & Assert
        await _service.Invoking(s => s.GetProductByIdAsync(0))
            .Should().ThrowAsync<ArgumentException>()
            .WithMessage("*ID must be greater than 0*");

        await _service.Invoking(s => s.GetProductByIdAsync(-1))
            .Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task CreateProductAsync_WithValidData_CreatesProduct()
    {
        // Arrange
        var dto = new CreateProductDto("New Product", "Description", 49.99m, 20);
        var expectedProduct = new Product(1, dto.Name, dto.Description, dto.Price, dto.Stock);
        
        _mockRepository.CreateAsync(Arg.Any<Product>()).Returns(expectedProduct);

        // Act
        var result = await _service.CreateProductAsync(dto);

        // Assert
        result.Should().BeEquivalentTo(expectedProduct);
        await _mockRepository.Received(1).CreateAsync(Arg.Is<Product>(p =>
            p.Name == dto.Name &&
            p.Description == dto.Description &&
            p.Price == dto.Price &&
            p.Stock == dto.Stock));
    }

    [Theory]
    [InlineData("", "Description", 10, 5)]
    [InlineData(null, "Description", 10, 5)]
    [InlineData("   ", "Description", 10, 5)]
    public async Task CreateProductAsync_WithInvalidName_ThrowsArgumentException(
        string name, string description, decimal price, int stock)
    {
        // Arrange
        var dto = new CreateProductDto(name, description, price, stock);

        // Act & Assert
        await _service.Invoking(s => s.CreateProductAsync(dto))
            .Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Name is required*");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public async Task CreateProductAsync_WithInvalidPrice_ThrowsArgumentException(decimal price)
    {
        // Arrange
        var dto = new CreateProductDto("Product", "Description", price, 10);

        // Act & Assert
        await _service.Invoking(s => s.CreateProductAsync(dto))
            .Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Price must be greater than 0*");
    }

    [Fact]
    public async Task UpdateProductAsync_WithValidData_UpdatesProduct()
    {
        // Arrange
        var productId = 1;
        var dto = new UpdateProductDto("Updated", "New Description", 199.99m, 15);
        var existingProduct = new Product(productId, "Old", "Old Desc", 99.99m, 10);
        var updatedProduct = new Product(productId, dto.Name, dto.Description, dto.Price, dto.Stock);

        _mockRepository.GetByIdAsync(productId).Returns(existingProduct);
        _mockRepository.UpdateAsync(productId, Arg.Any<Product>()).Returns(updatedProduct);

        // Act
        var result = await _service.UpdateProductAsync(productId, dto);

        // Assert
        result.Should().BeEquivalentTo(updatedProduct);
        await _mockRepository.Received(1).GetByIdAsync(productId);
        await _mockRepository.Received(1).UpdateAsync(productId, Arg.Any<Product>());
    }

    [Fact]
    public async Task UpdateProductAsync_WithNonExistentId_ReturnsNull()
    {
        // Arrange
        var productId = 999;
        var dto = new UpdateProductDto("Updated", "Description", 99.99m, 10);
        _mockRepository.GetByIdAsync(productId).Returns((Product?)null);

        // Act
        var result = await _service.UpdateProductAsync(productId, dto);

        // Assert
        result.Should().BeNull();
        await _mockRepository.Received(1).GetByIdAsync(productId);
        await _mockRepository.DidNotReceive().UpdateAsync(Arg.Any<int>(), Arg.Any<Product>());
    }

    [Fact]
    public async Task DeleteProductAsync_WithValidId_DeletesProduct()
    {
        // Arrange
        var productId = 1;
        _mockRepository.DeleteAsync(productId).Returns(true);

        // Act
        var result = await _service.DeleteProductAsync(productId);

        // Assert
        result.Should().BeTrue();
        await _mockRepository.Received(1).DeleteAsync(productId);
    }

    [Fact]
    public async Task DeleteProductAsync_WithInvalidId_ThrowsArgumentException()
    {
        // Act & Assert
        await _service.Invoking(s => s.DeleteProductAsync(0))
            .Should().ThrowAsync<ArgumentException>()
            .WithMessage("*ID must be greater than 0*");

        await _mockRepository.DidNotReceive().DeleteAsync(Arg.Any<int>());
    }
}


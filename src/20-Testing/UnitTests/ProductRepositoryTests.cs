using FluentAssertions;
using Xunit;

namespace Testing.Tests.UnitTests;

/// <summary>
/// Unit tests for InMemoryProductRepository
/// Tests data access layer
/// </summary>
public class ProductRepositoryTests
{
    [Fact]
    public async Task GetAllAsync_ReturnsSeededProducts()
    {
        // Arrange
        var repository = new InMemoryProductRepository();

        // Act
        var products = await repository.GetAllAsync();

        // Assert
        products.Should().NotBeEmpty();
        products.Should().HaveCountGreaterThan(0);
        products.Should().AllSatisfy(p =>
        {
            p.Id.Should().BeGreaterThan(0);
            p.Name.Should().NotBeNullOrWhiteSpace();
        });
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingId_ReturnsProduct()
    {
        // Arrange
        var repository = new InMemoryProductRepository();
        var allProducts = await repository.GetAllAsync();
        var firstProduct = allProducts.First();

        // Act
        var result = await repository.GetByIdAsync(firstProduct.Id);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(firstProduct);
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistentId_ReturnsNull()
    {
        // Arrange
        var repository = new InMemoryProductRepository();

        // Act
        var result = await repository.GetByIdAsync(9999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_AddsNewProduct()
    {
        // Arrange
        var repository = new InMemoryProductRepository();
        var newProduct = new Product(0, "New Product", "Description", 99.99m, 10);

        // Act
        var created = await repository.CreateAsync(newProduct);

        // Assert
        created.Should().NotBeNull();
        created.Id.Should().BeGreaterThan(0);
        created.Name.Should().Be(newProduct.Name);
        created.Price.Should().Be(newProduct.Price);

        // Verify it's in the repository
        var retrieved = await repository.GetByIdAsync(created.Id);
        retrieved.Should().BeEquivalentTo(created);
    }

    [Fact]
    public async Task UpdateAsync_WithExistingId_UpdatesProduct()
    {
        // Arrange
        var repository = new InMemoryProductRepository();
        var allProducts = await repository.GetAllAsync();
        var existingProduct = allProducts.First();
        var updatedProduct = existingProduct with
        {
            Name = "Updated Name",
            Price = 999.99m
        };

        // Act
        var result = await repository.UpdateAsync(existingProduct.Id, updatedProduct);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Updated Name");
        result.Price.Should().Be(999.99m);

        // Verify the update persisted
        var retrieved = await repository.GetByIdAsync(existingProduct.Id);
        retrieved.Should().BeEquivalentTo(result);
    }

    [Fact]
    public async Task UpdateAsync_WithNonExistentId_ReturnsNull()
    {
        // Arrange
        var repository = new InMemoryProductRepository();
        var product = new Product(9999, "Test", "Description", 99.99m, 10);

        // Act
        var result = await repository.UpdateAsync(9999, product);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_WithExistingId_RemovesProduct()
    {
        // Arrange
        var repository = new InMemoryProductRepository();
        var allProducts = await repository.GetAllAsync();
        var productToDelete = allProducts.First();

        // Act
        var result = await repository.DeleteAsync(productToDelete.Id);

        // Assert
        result.Should().BeTrue();

        // Verify it's deleted
        var retrieved = await repository.GetByIdAsync(productToDelete.Id);
        retrieved.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_WithNonExistentId_ReturnsFalse()
    {
        // Arrange
        var repository = new InMemoryProductRepository();

        // Act
        var result = await repository.DeleteAsync(9999);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task CreateAsync_GeneratesUniqueIds()
    {
        // Arrange
        var repository = new InMemoryProductRepository();
        var product1 = new Product(0, "Product 1", "Desc", 10m, 5);
        var product2 = new Product(0, "Product 2", "Desc", 20m, 10);

        // Act
        var created1 = await repository.CreateAsync(product1);
        var created2 = await repository.CreateAsync(product2);

        // Assert
        created1.Id.Should().NotBe(created2.Id);
        created1.Id.Should().BeGreaterThan(0);
        created2.Id.Should().BeGreaterThan(0);
    }
}


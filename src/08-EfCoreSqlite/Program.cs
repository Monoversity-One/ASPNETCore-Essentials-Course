using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

var builder = WebApplication.CreateBuilder(args);

// Configure DbContext with SQLite
builder.Services.AddDbContext<StoreDb>(opt =>
{
    opt.UseSqlite(builder.Configuration.GetConnectionString("sqlite") ?? "Data Source=store.db");
    opt.EnableSensitiveDataLogging(builder.Environment.IsDevelopment());
    opt.EnableDetailedErrors(builder.Environment.IsDevelopment());
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Ensure database and seed data
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<StoreDb>();
    db.Database.EnsureCreated();

    if (!db.Products.Any())
    {
        // Seed categories
        var electronics = new Category { Name = "Electronics", Description = "Electronic devices and accessories" };
        var office = new Category { Name = "Office", Description = "Office supplies" };

        db.Categories.AddRange(electronics, office);
        await db.SaveChangesAsync();

        // Seed products
        db.Products.AddRange(
            new Product
            {
                Name = "Laptop",
                Description = "High-performance laptop",
                Price = 1299.99m,
                Stock = 15,
                CategoryId = electronics.Id,
                CreatedAt = DateTime.UtcNow
            },
            new Product
            {
                Name = "Wireless Mouse",
                Description = "Ergonomic wireless mouse",
                Price = 29.99m,
                Stock = 50,
                CategoryId = electronics.Id,
                CreatedAt = DateTime.UtcNow
            },
            new Product
            {
                Name = "Mechanical Keyboard",
                Description = "RGB mechanical keyboard",
                Price = 89.99m,
                Stock = 30,
                CategoryId = electronics.Id,
                CreatedAt = DateTime.UtcNow
            },
            new Product
            {
                Name = "Notebook",
                Description = "A5 ruled notebook",
                Price = 4.99m,
                Stock = 100,
                CategoryId = office.Id,
                CreatedAt = DateTime.UtcNow
            }
        );
        await db.SaveChangesAsync();

        // Seed customers and orders
        var customer = new Customer
        {
            Name = "John Doe",
            Email = "john@example.com",
            CreatedAt = DateTime.UtcNow
        };
        db.Customers.Add(customer);
        await db.SaveChangesAsync();

        var order = new Order
        {
            CustomerId = customer.Id,
            OrderDate = DateTime.UtcNow,
            Status = "Pending"
        };
        db.Orders.Add(order);
        await db.SaveChangesAsync();
    }
}

// ===== PRODUCT ENDPOINTS =====

// Get all products with optional filtering and pagination
app.MapGet("/products", async (StoreDb db, int? categoryId, decimal? minPrice, decimal? maxPrice, int page = 1, int pageSize = 10) =>
{
    var query = db.Products.Include(p => p.Category).AsNoTracking();

    if (categoryId.HasValue)
        query = query.Where(p => p.CategoryId == categoryId);

    if (minPrice.HasValue)
        query = query.Where(p => p.Price >= minPrice);

    if (maxPrice.HasValue)
        query = query.Where(p => p.Price <= maxPrice);

    var total = await query.CountAsync();
    var products = await query
        .OrderBy(p => p.Name)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync();

    return Results.Ok(new { total, page, pageSize, products });
})
    .WithName("GetProducts")
    .WithTags("Products");

// Get product by ID with related data
app.MapGet("/products/{id:int}", async (int id, StoreDb db) =>
{
    var product = await db.Products
        .Include(p => p.Category)
        .AsNoTracking()
        .FirstOrDefaultAsync(p => p.Id == id);

    return product is not null ? Results.Ok(product) : Results.NotFound();
})
    .WithName("GetProduct")
    .WithTags("Products");

// Create product
app.MapPost("/products", async (CreateProductDto dto, StoreDb db) =>
{
    var product = new Product
    {
        Name = dto.Name,
        Description = dto.Description,
        Price = dto.Price,
        Stock = dto.Stock,
        CategoryId = dto.CategoryId,
        CreatedAt = DateTime.UtcNow
    };

    db.Products.Add(product);
    await db.SaveChangesAsync();

    return Results.Created($"/products/{product.Id}", product);
})
    .WithName("CreateProduct")
    .WithTags("Products");

// Update product
app.MapPut("/products/{id:int}", async (int id, UpdateProductDto dto, StoreDb db) =>
{
    var product = await db.Products.FindAsync(id);
    if (product is null) return Results.NotFound();

    product.Name = dto.Name;
    product.Description = dto.Description;
    product.Price = dto.Price;
    product.Stock = dto.Stock;
    product.CategoryId = dto.CategoryId;
    product.UpdatedAt = DateTime.UtcNow;

    await db.SaveChangesAsync();
    return Results.NoContent();
})
    .WithName("UpdateProduct")
    .WithTags("Products");

// Delete product
app.MapDelete("/products/{id:int}", async (int id, StoreDb db) =>
{
    var product = await db.Products.FindAsync(id);
    if (product is null) return Results.NotFound();

    db.Products.Remove(product);
    await db.SaveChangesAsync();
    return Results.NoContent();
})
    .WithName("DeleteProduct")
    .WithTags("Products");

// ===== CATEGORY ENDPOINTS =====

app.MapGet("/categories", async (StoreDb db) =>
    await db.Categories.Include(c => c.Products).AsNoTracking().ToListAsync())
    .WithName("GetCategories")
    .WithTags("Categories");

app.MapGet("/categories/{id:int}", async (int id, StoreDb db) =>
{
    var category = await db.Categories
        .Include(c => c.Products)
        .AsNoTracking()
        .FirstOrDefaultAsync(c => c.Id == id);

    return category is not null ? Results.Ok(category) : Results.NotFound();
})
    .WithName("GetCategory")
    .WithTags("Categories");

app.MapPost("/categories", async (CreateCategoryDto dto, StoreDb db) =>
{
    var category = new Category { Name = dto.Name, Description = dto.Description };
    db.Categories.Add(category);
    await db.SaveChangesAsync();
    return Results.Created($"/categories/{category.Id}", category);
})
    .WithName("CreateCategory")
    .WithTags("Categories");

// ===== CUSTOMER & ORDER ENDPOINTS =====

app.MapGet("/customers", async (StoreDb db) =>
    await db.Customers.AsNoTracking().ToListAsync())
    .WithName("GetCustomers")
    .WithTags("Customers");

app.MapGet("/customers/{id:int}/orders", async (int id, StoreDb db) =>
{
    var orders = await db.Orders
        .Where(o => o.CustomerId == id)
        .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
        .AsNoTracking()
        .ToListAsync();

    return Results.Ok(orders);
})
    .WithName("GetCustomerOrders")
    .WithTags("Customers");

// ===== ADVANCED QUERIES =====

// Search products by name
app.MapGet("/products/search", async (string query, StoreDb db) =>
{
    var products = await db.Products
        .Where(p => EF.Functions.Like(p.Name, $"%{query}%") ||
                    EF.Functions.Like(p.Description, $"%{query}%"))
        .AsNoTracking()
        .ToListAsync();

    return Results.Ok(products);
})
    .WithName("SearchProducts")
    .WithTags("Products");

// Get low stock products
app.MapGet("/products/low-stock", async (int threshold, StoreDb db) =>
{
    var products = await db.Products
        .Where(p => p.Stock <= threshold)
        .OrderBy(p => p.Stock)
        .AsNoTracking()
        .ToListAsync();

    return Results.Ok(products);
})
    .WithName("GetLowStockProducts")
    .WithTags("Products");

// Get statistics
app.MapGet("/stats", async (StoreDb db) =>
{
    var stats = new
    {
        totalProducts = await db.Products.CountAsync(),
        totalCategories = await db.Categories.CountAsync(),
        totalCustomers = await db.Customers.CountAsync(),
        totalOrders = await db.Orders.CountAsync(),
        averageProductPrice = await db.Products.AverageAsync(p => p.Price),
        totalInventoryValue = await db.Products.SumAsync(p => p.Price * p.Stock)
    };

    return Results.Ok(stats);
})
    .WithName("GetStatistics")
    .WithTags("Statistics");

app.Run();


// ===== DATABASE CONTEXT =====

class StoreDb : DbContext
{
    public StoreDb(DbContextOptions<StoreDb> options) : base(options) { }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Product configuration
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Price).HasPrecision(18, 2);
            entity.HasIndex(e => e.Name);

            // Relationship: Product belongs to Category
            entity.HasOne(e => e.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Category configuration
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
        });

        // Customer configuration
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(200);
            entity.HasIndex(e => e.Email).IsUnique();
        });

        // Order configuration
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id);

            // Relationship: Order belongs to Customer
            entity.HasOne(e => e.Customer)
                .WithMany(c => c.Orders)
                .HasForeignKey(e => e.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // OrderItem configuration
        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UnitPrice).HasPrecision(18, 2);

            // Relationship: OrderItem belongs to Order
            entity.HasOne(e => e.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(e => e.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // Relationship: OrderItem references Product
            entity.HasOne(e => e.Product)
                .WithMany()
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}

// ===== ENTITY MODELS =====

class Product
{
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;

    [Range(0.01, double.MaxValue)]
    public decimal Price { get; set; }

    [Range(0, int.MaxValue)]
    public int Stock { get; set; }

    public int CategoryId { get; set; }
    public Category? Category { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

class Category
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    public ICollection<Product> Products { get; set; } = new List<Product>();
}

class Customer
{
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(200)]
    public string Email { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public ICollection<Order> Orders { get; set; } = new List<Order>();
}

class Order
{
    public int Id { get; set; }

    public int CustomerId { get; set; }
    public Customer? Customer { get; set; }

    public DateTime OrderDate { get; set; }

    [MaxLength(50)]
    public string Status { get; set; } = "Pending";

    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}

class OrderItem
{
    public int Id { get; set; }

    public int OrderId { get; set; }
    public Order? Order { get; set; }

    public int ProductId { get; set; }
    public Product? Product { get; set; }

    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }
}

// ===== DTOs =====

record CreateProductDto(
    [Required] string Name,
    string Description,
    [Range(0.01, double.MaxValue)] decimal Price,
    [Range(0, int.MaxValue)] int Stock,
    int CategoryId);

record UpdateProductDto(
    [Required] string Name,
    string Description,
    [Range(0.01, double.MaxValue)] decimal Price,
    [Range(0, int.MaxValue)] int Stock,
    int CategoryId);

record CreateCategoryDto(
    [Required] string Name,
    string Description);

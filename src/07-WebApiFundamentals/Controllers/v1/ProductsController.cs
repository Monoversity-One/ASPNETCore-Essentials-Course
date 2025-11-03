using System.ComponentModel.DataAnnotations;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace WebApiFundamentals.Controllers.v1;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class ProductsController : ControllerBase
{
    private static readonly List<Product> Products = new()
    {
        new Product { Id = 1, Name = "Coffee", Price = 3.99m },
        new Product { Id = 2, Name = "Tea", Price = 2.49m },
    };

    [HttpGet]
    public ActionResult<IEnumerable<Product>> GetAll() => Ok(Products);

    [HttpGet("{id:int}")]
    public ActionResult<Product> GetById(int id)
    {
        var product = Products.FirstOrDefault(p => p.Id == id);
        return product is null ? NotFound() : Ok(product);
    }

    [HttpPost]
    public ActionResult<Product> Create([FromBody] CreateProduct dto)
    {
        var id = Products.Count == 0 ? 1 : Products.Max(p => p.Id) + 1;
        var product = new Product { Id = id, Name = dto.Name, Price = dto.Price };
        Products.Add(product);
        return CreatedAtAction(nameof(GetById), new { id, version = HttpContext.GetRequestedApiVersion()!.ToString() }, product);
    }
}

public class CreateProduct
{
    [Required, StringLength(50)]
    public string Name { get; set; } = string.Empty;

    [Range(0.01, 1000)]
    public decimal Price { get; set; }
}

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.ComponentModel.DataAnnotations;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers(options =>
{
    options.Filters.Add(new GlobalLoggingFilter());
});

var app = builder.Build();

app.MapControllers();

app.Run();

public class GlobalLoggingFilter : IActionFilter, IExceptionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        Console.WriteLine($"Executing {context.ActionDescriptor.DisplayName}");
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        Console.WriteLine($"Executed {context.ActionDescriptor.DisplayName} with {context.HttpContext.Response.StatusCode}");
    }

    public void OnException(ExceptionContext context)
    {
        context.Result = new ObjectResult(new { error = context.Exception.Message }) { StatusCode = 500 };
    }
}

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    [HttpPost]
    public IActionResult Create([FromBody] CreateOrder dto)
    {
        return Ok(new { id = 1, dto.Product, dto.Quantity });
    }
}

public class CreateOrder
{
    [Required]
    public string Product { get; set; } = string.Empty;

    [Range(1, 100)]
    public int Quantity { get; set; }
}

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace RazorPagesSample.Pages;

public class IndexModel : PageModel
{
    [BindProperty]
    [StringLength(50, MinimumLength = 1)]
    public string? Input { get; set; }

    public string? Message { get; set; }
    public DateTimeOffset Now { get; private set; } = DateTimeOffset.UtcNow;

    public void OnGet()
    {
        Now = DateTimeOffset.UtcNow;
    }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }
        Message = $"You typed: {Input}";
        return Page();
    }
}

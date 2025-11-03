using Microsoft.AspNetCore.Mvc;

namespace MvcControllersViews.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        ViewData["Title"] = "Home";
        return View();
    }

    public IActionResult About()
    {
        ViewBag.Message = "This is an MVC controllers & views sample.";
        return View();
    }
}

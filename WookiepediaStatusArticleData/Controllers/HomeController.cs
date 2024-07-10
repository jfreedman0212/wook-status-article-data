using Microsoft.AspNetCore.Mvc;

namespace WookiepediaStatusArticleData.Controllers;

[Route("/")]
public class HomeController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }
}
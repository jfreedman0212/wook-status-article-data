using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using WookiepediaStatusArticleData.Models;

namespace WookiepediaStatusArticleData.Controllers;

[Route("/")]
public class HomeController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }
    
    [Route("/home/error")]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
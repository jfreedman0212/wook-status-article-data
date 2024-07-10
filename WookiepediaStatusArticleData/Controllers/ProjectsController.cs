using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WookiepediaStatusArticleData.Database;
using WookiepediaStatusArticleData.Models;
using WookiepediaStatusArticleData.Models.Projects;
using WookiepediaStatusArticleData.Nominations;

namespace WookiepediaStatusArticleData.Controllers;

[Authorize]
[Route("/projects")]
public class ProjectsController(WookiepediaDbContext db) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var projects = await db.Set<Project>().ToListAsync(cancellationToken);

        return View(new ProjectViewModel
        {
            Projects = projects
        });
    }
    
    [HttpGet("create")]
    public IActionResult CreateForm()
    {
        return View();
    }

    public async Task<IActionResult> Create(
        [FromForm] ProjectForm form,
        CancellationToken cancellationToken    
    )
    {
        var project = new Project()
        {
            Name = form.Name
        };

        db.Add(project);
        await db.SaveChangesAsync(cancellationToken);

        return NoContent(); // TODO: return partial view!!
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
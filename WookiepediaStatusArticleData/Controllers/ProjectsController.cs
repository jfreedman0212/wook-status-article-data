using Htmx;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WookiepediaStatusArticleData.Database;
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
        var projects = await db.Set<Project>()
            .OrderBy(it => it.Name)
            .ToListAsync(cancellationToken);

        ModelState.Clear();
        return View(new ProjectsViewModel { Projects = projects });
    }
    
    [HttpGet("add-form")]
    public IActionResult AddForm()
    {
        return PartialView("_Project.Add", null);
    }

    [HttpGet("add-button")]
    public IActionResult AddButton()
    {
        return PartialView("_AddButton");
    }
    
    [HttpGet("{id:int}")]
    public async Task<IActionResult> ProjectPartial(
        [FromRoute] int id,
        CancellationToken cancellationToken
    )
    {
        var project = await db.Set<Project>().SingleOrDefaultAsync(it => it.Id == id, cancellationToken);

        if (project == null) return NotFound();

        return PartialView("_Project", project);
    }

    [HttpGet("edit/{id:int}")]
    public async Task<IActionResult> EditForm(
        [FromRoute] int id,
        CancellationToken cancellationToken
    )
    {
        var project = await db.Set<Project>().SingleOrDefaultAsync(it => it.Id == id, cancellationToken);

        if (project == null) return NotFound();

        return PartialView("_Project.Edit", new ProjectForm
        {
            Id = project.Id,
            Name = project.Name,
            CreatedDate = DateOnly.FromDateTime(project.CreatedAt),
            CreatedTime = TimeOnly.FromDateTime(project.CreatedAt)
        });
    }
    
    [HttpPost("edit/{id:int}")]
    public async Task<IActionResult> Edit(
        [FromRoute] int id,
        [FromForm] ProjectForm form,
        CancellationToken cancellationToken    
    )
    {
        form.Id = id;

        var project = await db.Set<Project>().SingleOrDefaultAsync(it => it.Id == id, cancellationToken);

        if (project == null) return NotFound();

        var differentProjectWithSameName = await db.Set<Project>()
            .SingleOrDefaultAsync(it => it.Id != id && it.Name == form.Name, cancellationToken);

        if (differentProjectWithSameName != null)
        {
            ModelState.AddModelError(nameof(form.Name), $"{form.Name} already exists, choose another name.");
        }

        if (!ModelState.IsValid)
        {
            Response.StatusCode = 400;
            return PartialView("_Project.Edit", form);
        }

        project.Name = form.Name;
        project.CreatedAt = new DateTime(form.CreatedDate, form.CreatedTime, DateTimeKind.Utc);
        
        await db.SaveChangesAsync(cancellationToken);

        return PartialView("_Project", project);
    }

    [HttpPost("create")]
    public async Task<IActionResult> Create(
        [FromForm] ProjectForm form,
        CancellationToken cancellationToken    
    )
    {
        var project = new Project
        {
            Name = form.Name,
            CreatedAt = new DateTime(form.CreatedDate, form.CreatedTime, DateTimeKind.Utc)
        };
        
        var differentProjectWithSameName = await db.Set<Project>()
            .SingleOrDefaultAsync(it => it.Name == form.Name, cancellationToken);

        if (differentProjectWithSameName != null)
        {
            ModelState.AddModelError(nameof(form.Name), $"{form.Name} already exists, choose another name.");
        }

        if (!ModelState.IsValid)
        {
            Response.StatusCode = 400;
            // we want it to just replace the form itself and not append the new data
            // to the end if the form isn't valid
            Response.Htmx(headers =>
            {
                headers.Retarget("#project-add-form");
                headers.Reswap("outerHTML");
            });
            return PartialView("_Project.Add", form);
        }

        db.Add(project);
        await db.SaveChangesAsync(cancellationToken);

        var projects = await db.Set<Project>()
            .OrderBy(it => it.Name)
            .ToListAsync(cancellationToken);
        
        ModelState.Clear();
        return PartialView("Index", new ProjectsViewModel { Projects = projects });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(
        [FromRoute] int id,
        CancellationToken cancellationToken
    )
    {
        var project = await db.Set<Project>().SingleOrDefaultAsync(it => it.Id == id, cancellationToken);

        if (project == null) return Ok();

        db.Remove(project);
        await db.SaveChangesAsync(cancellationToken);
        
        return Ok();
    }
}
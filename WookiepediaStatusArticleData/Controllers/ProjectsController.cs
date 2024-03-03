using Htmx;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WookiepediaStatusArticleData.Database;
using WookiepediaStatusArticleData.Models.Projects;
using WookiepediaStatusArticleData.Nominations.Projects;

namespace WookiepediaStatusArticleData.Controllers;

[Authorize]
[Route("/projects")]
public class ProjectsController(WookiepediaDbContext db) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var projects = await db.Set<Project>()
            .Where(it => !it.IsArchived)
            .OrderBy(it => it.Name)
            .ToListAsync(cancellationToken);

        ModelState.Clear();
        return View(new ProjectsViewModel { Projects = projects });
    }
    
    [HttpGet("add-form")]
    public IActionResult AddForm()
    {
        var now = DateTime.UtcNow;
        return PartialView("_Project.Add", new ProjectForm
        {
            Name = "",
            Type = ProjectType.Category,
            CreatedDate = DateOnly.FromDateTime(now),
            CreatedTime = TimeOnly.FromDateTime(now)
        });
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
        var project = await db.Set<Project>()
            .SingleOrDefaultAsync(it => it.Id == id && !it.IsArchived, cancellationToken);

        if (project == null) return NotFound();

        return PartialView("_Project", project);
    }

    [HttpGet("edit/{id:int}")]
    public async Task<IActionResult> EditForm(
        [FromRoute] int id,
        CancellationToken cancellationToken
    )
    {
        var project = await db.Set<Project>().SingleOrDefaultAsync(it => it.Id == id && !it.IsArchived, cancellationToken);

        if (project == null) return NotFound();

        return PartialView("_Project.Edit", new ProjectForm
        {
            Id = project.Id,
            Name = project.Name,
            Type = project.Type,
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

        var project = await db.Set<Project>()
            .Include(it => it.HistoricalValues)
            .SingleOrDefaultAsync(it => it.Id == id && !it.IsArchived, cancellationToken);

        if (project == null) return NotFound();

        var differentProjectWithSameName = await db.Set<Project>()
            .SingleOrDefaultAsync(it => it.Id != id && it.Name == form.Name, cancellationToken);

        if (differentProjectWithSameName != null)
        {
            ModelState.AddModelError(
                nameof(form.Name),
                differentProjectWithSameName.IsArchived
                    ? $"{form.Name} already exists as an archived project, choose another name."
                    : $"{form.Name} already exists, choose another name."
            );
        }

        if (!ModelState.IsValid)
        {
            Response.StatusCode = 400;
            return PartialView("_Project.Edit", form);
        }

        project.Name = form.Name;
        project.Type = form.Type;
        project.HistoricalValues!.Add(new HistoricalProject
        {
            ActionType = ProjectActionType.Update,
            Name = form.Name,
            Type = form.Type,
            OccurredAt = DateTime.UtcNow
        });
        
        await db.SaveChangesAsync(cancellationToken);

        return PartialView("_Project", project);
    }

    [HttpPost("create")]
    public async Task<IActionResult> Create(
        [FromForm] ProjectForm form,
        CancellationToken cancellationToken    
    )
    {
        var createdAt = new DateTime(form.CreatedDate, form.CreatedTime, DateTimeKind.Utc);
        var project = new Project
        {
            Name = form.Name,
            CreatedAt = createdAt,
            Type = form.Type,
            HistoricalValues = 
            [
                new HistoricalProject
                { 
                    ActionType = ProjectActionType.Create,
                    Name = form.Name,
                    Type = form.Type,
                    OccurredAt = createdAt
                }
            ]
        };

        var now = DateTime.UtcNow;
        var nowDate = DateOnly.FromDateTime(now);
        var nowTime = TimeOnly.FromDateTime(now);
        
        if (nowDate < form.CreatedDate)
        {
            ModelState.AddModelError(nameof(form.CreatedDate), "Created Date must be today or in the past.");
        }

        if (nowDate == form.CreatedDate && nowTime < form.CreatedTime)
        {
            ModelState.AddModelError(nameof(form.CreatedTime), "Created Time must be now or in the past.");    
        }
        
        var differentProjectWithSameName = await db.Set<Project>()
            .SingleOrDefaultAsync(it => it.Name == form.Name, cancellationToken);

        if (differentProjectWithSameName != null)
        {
            ModelState.AddModelError(
                nameof(form.Name),
                differentProjectWithSameName.IsArchived
                    ? $"{form.Name} already exists as an archived project, choose another name."
                    : $"{form.Name} already exists, choose another name."
            );
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
        var project = await db.Set<Project>()
            .Include(it => it.HistoricalValues)
            .SingleOrDefaultAsync(it => it.Id == id && !it.IsArchived, cancellationToken);

        if (project == null) return Ok();

        project.IsArchived = true;
        project.HistoricalValues!.Add(new HistoricalProject
        {
            Name = project.Name,
            Type = project.Type,
            ActionType = ProjectActionType.Archive,
            OccurredAt = DateTime.UtcNow
        });
        
        await db.SaveChangesAsync(cancellationToken);
        
        return Ok();
    }
}
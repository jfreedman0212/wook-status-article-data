using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WookiepediaStatusArticleData.Database;
using WookiepediaStatusArticleData.Models.Projects;
using WookiepediaStatusArticleData.Nominations.Projects;
using WookiepediaStatusArticleData.Services;
using WookiepediaStatusArticleData.Services.Projects;

namespace WookiepediaStatusArticleData.Controllers;

[Authorize]
[Route("projects")]
public class ProjectsController(WookiepediaDbContext db) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(
        [FromQuery] bool isArchived,
        CancellationToken cancellationToken
    )
    {
        var projects = await db.Set<Project>()
            .Where(it => it.IsArchived == isArchived)
            .OrderBy(it => it.Name)
            .ToListAsync(cancellationToken);

        return View(new ProjectsViewModel { Projects = projects });
    }

    [HttpGet("new")]
    public IActionResult AddForm()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Add(
        [FromForm] ProjectForm form,
        [FromServices] CreateProjectAction action,
        CancellationToken cancellationToken
    )
    {
        if (!ModelState.IsValid)
        {
            Response.StatusCode = 400;
            return View("AddForm", form);
        }

        try
        {
            await action.ExecuteAsync(form, cancellationToken);

            await db.SaveChangesAsync(cancellationToken);
            return RedirectToAction("Index");
        }
        catch (ValidationException validationException)
        {
            foreach (var issue in validationException.Issues)
            {
                ModelState.AddModelError(issue.Name, issue.Message);
            }

            Response.StatusCode = 400;
            return View("AddForm", form);
        }
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> EditForm(
        [FromRoute] int id,
        CancellationToken cancellationToken
    )
    {
        var project = await db.Set<Project>().SingleOrDefaultAsync(it => it.Id == id, cancellationToken);

        if (project == null) return NotFound();

        return View(new ProjectForm
        {
            Id = project.Id,
            Name = project.Name,
            Type = project.Type,
            CreatedDate = DateOnly.FromDateTime(project.CreatedAt),
            CreatedTime = TimeOnly.FromDateTime(project.CreatedAt)
        });
    }

    [HttpPost("{id:int}")]
    public async Task<IActionResult> Edit(
        [FromRoute] int id,
        [FromForm] ProjectForm form,
        [FromServices] EditProjectAction action,
        CancellationToken cancellationToken
    )
    {
        if (!ModelState.IsValid)
        {
            Response.StatusCode = 400;
            return View("EditForm", form);
        }

        try
        {
            var project = await action.ExecuteAsync(id, form, cancellationToken);

            if (project == null) return NotFound();

            await db.SaveChangesAsync(cancellationToken);
            return RedirectToAction("Index");
        }
        catch (ValidationException validationException)
        {
            foreach (var issue in validationException.Issues)
            {
                ModelState.AddModelError(issue.Name, issue.Message);
            }

            Response.StatusCode = 400;
            return View("EditForm", form);
        }
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

    [HttpGet("merge")]
    public async Task<IActionResult> MergeForm(CancellationToken cancellationToken)
    {
        var allProjects = await db.Set<Project>()
            .Where(it => !it.IsArchived)
            .ToListAsync(cancellationToken);

        return View(new MergeProjectForm
        {
            AllProjects = allProjects
                .Select(it => new SelectListItem(it.Name, it.Id.ToString()))
                .ToList()
        });
    }

    [HttpPost("merge")]
    public async Task<IActionResult> Merge(
        [FromForm] MergeProjectForm form,
        CancellationToken cancellationToken
    )
    {
        var fromProject = await db.Set<Project>().SingleOrDefaultAsync(it => it.Id == form.FromProjectId, cancellationToken);

        if (fromProject == null)
        {
            ModelState.AddModelError(nameof(form.FromProjectId), $"{form.FromProjectId} does not exist.");
        }

        var toProject = await db.Set<Project>().SingleOrDefaultAsync(it => it.Id == form.ToProjectId, cancellationToken);

        if (toProject == null)
        {
            ModelState.AddModelError(nameof(form.ToProjectId), $"{form.ToProjectId} does not exist.");
        }
        
        if (fromProject != null && toProject != null && fromProject.Id == toProject.Id)
        {
            ModelState.AddModelError(nameof(form.ToProjectId), $"You must choose two different projects.");
        }

        if (!ModelState.IsValid)
        {
            var allProjects = await db.Set<Project>()
                .Where(it => !it.IsArchived)
                .ToListAsync(cancellationToken);

            form.AllProjects = allProjects
                .Select(it => new SelectListItem(it.Name, it.Id.ToString()))
                .ToList();

            Response.StatusCode = 400;
            return View("MergeForm", form);
        }

        // TODO: remove historical data (or keep it?)
        // TODO: associate with nominations
        // TODO: delete project_awards records and regenerate the affected generation groups

        // TODO: longer term, offload this to a worker via Mass Transit

        return RedirectToAction("Index");
    }
}
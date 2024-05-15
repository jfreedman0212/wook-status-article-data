using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WookiepediaStatusArticleData.Database;
using WookiepediaStatusArticleData.Models.Projects;
using WookiepediaStatusArticleData.Nominations.Projects;
using WookiepediaStatusArticleData.Services;
using WookiepediaStatusArticleData.Services.Projects;

namespace WookiepediaStatusArticleData.Controllers;

[Authorize]
[ApiController]
[Route("projects")]
public class ProjectsController(WookiepediaDbContext db) : ControllerBase
{
    [HttpGet]
    public async IAsyncEnumerable<ProjectViewModel> Index(
        [EnumeratorCancellation] CancellationToken cancellationToken
    )
    {
        var projects = db.Set<Project>()
            .Where(it => !it.IsArchived)
            .OrderBy(it => it.Name)
            .Select(it => new ProjectViewModel
            {
                Id = it.Id,
                Name = it.Name,
                Type = it.Type,
                CreatedAt = it.CreatedAt
            })
            .AsAsyncEnumerable()
            .WithCancellation(cancellationToken);

        await foreach (var project in projects)
        {
            yield return project;
        }
    }
    
    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(
        [FromRoute] int id,
        CancellationToken cancellationToken
    )
    {
        var project = await db.Set<Project>()
            .Where(it => it.Id == id && !it.IsArchived)
            .Select(it => new ProjectViewModel
            {
                Id = it.Id,
                Name = it.Name,
                Type = it.Type,
                CreatedAt = it.CreatedAt
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (project == null) return NotFound();

        return Ok(project);
    }
    
    [HttpPost("{id:int}")]
    public async Task<IActionResult> Edit(
        [FromRoute] int id,
        [FromBody] EditProjectForm form,
        [FromServices] EditProjectAction action,
        CancellationToken cancellationToken    
    )
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);   
        
        try
        {
            var project = await action.ExecuteAsync(id, form, cancellationToken);

            if (project == null) return NotFound();

            await db.SaveChangesAsync(cancellationToken);
            return NoContent(); // TODO ????
        }
        catch (ValidationException validationException)
        {
            foreach (var issue in validationException.Issues)
            {
                ModelState.AddModelError(issue.Name, issue.Message);
            }
            
            return ValidationProblem(ModelState);   
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] AddProjectForm form,
        [FromServices] CreateProjectAction action,
        CancellationToken cancellationToken    
    )
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);   
        
        try
        {
            await action.ExecuteAsync(form, cancellationToken);

            await db.SaveChangesAsync(cancellationToken);
            return NoContent(); // TODO ????
        }
        catch (ValidationException validationException)
        {
            foreach (var issue in validationException.Issues)
            {
                ModelState.AddModelError(issue.Name, issue.Message);
            }
            
            return ValidationProblem(ModelState);   
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

        if (project == null) return NoContent();

        project.IsArchived = true;
        project.HistoricalValues!.Add(new HistoricalProject
        {
            Name = project.Name,
            Type = project.Type,
            ActionType = ProjectActionType.Archive,
            OccurredAt = DateTime.UtcNow
        });
        
        await db.SaveChangesAsync(cancellationToken);
        
        return NoContent();
    }
}
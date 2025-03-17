using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WookiepediaStatusArticleData.Database;
using WookiepediaStatusArticleData.Models.Awards;
using WookiepediaStatusArticleData.Nominations.Awards;
using WookiepediaStatusArticleData.Services.Awards;
using WookiepediaStatusArticleData.Services.Awards.ProjectAwardCalculations;

namespace WookiepediaStatusArticleData.Controllers;

[Authorize]
[Route("award-generation-groups")]
public class AwardGenerationGroupsController(WookiepediaDbContext db) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var groups = await db.Set<AwardGenerationGroup>()
            .OrderByDescending(g => g.StartedAt)
            .ThenByDescending(g => g.EndedAt)
            .ThenBy(g => g.Name)
            .ToListAsync(cancellationToken);
        
        return View(new AwardGenerationGroupsViewModel { Groups = groups });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete([FromRoute] int id, CancellationToken cancellationToken)
    {
        await using var txn = await db.Database.BeginTransactionAsync(cancellationToken);
        
        await db.Set<Award>()
            .Where(g => g.GenerationGroupId == id)
            .ExecuteDeleteAsync(cancellationToken);
        
        await db.Set<AwardGenerationGroup>()
            .Where(g => g.Id == id)
            .ExecuteDeleteAsync(cancellationToken);

        await txn.CommitAsync(cancellationToken);
        
        return Ok();
    }

    [HttpGet("new")]
    public IActionResult CreateForm()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> CreateAsync(
        [FromForm] AwardGenerationGroupForm form,
        [FromServices] IEnumerable<IAwardGenerator> awardGenerators,
        [FromServices] IEnumerable<IProjectAwardCalculation> projectAwardCalculations,
        CancellationToken cancellationToken
    )
    {
        var startedAt = form.StartedAt.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var endedAt = form.EndedAt.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc);

        if (startedAt >= endedAt)
        {
            ModelState.AddModelError(nameof(form.StartedAt), "Started At must be before Ended At");
        }
        
        var nameAlreadyExists = await db.Set<AwardGenerationGroup>()
            .AnyAsync(
                c => c.Name == form.Name && c.StartedAt == startedAt && c.EndedAt == endedAt,
                cancellationToken
            );
        
        if (nameAlreadyExists)
        {
            ModelState.AddModelError(nameof(form.Name), $"{form.Name} already exists for that timeframe");
        }

        if (!ModelState.IsValid)
        {
            Response.StatusCode = 400;
            return View("CreateForm", form);
        }

        var now = DateTime.UtcNow;
        var newEntity = new AwardGenerationGroup
        {
            Name = form.Name,
            StartedAt = startedAt,
            EndedAt = endedAt,
            Awards = [],
            ProjectAwards = [],
            CreatedAt = now,
            UpdatedAt = now
        };

        await GenerateAwards(newEntity, awardGenerators, projectAwardCalculations, cancellationToken);

        db.Add(newEntity);
        await db.SaveChangesAsync(cancellationToken);
        
        return RedirectToAction("Index");
    }

    [HttpPost("{id:int}")]
    public async Task<IActionResult> RefreshAwards(
        [FromRoute] int id,
        [FromServices] IEnumerable<IAwardGenerator> awardGenerators,
        [FromServices] IEnumerable<IProjectAwardCalculation> projectAwardCalculations,
        CancellationToken cancellationToken
    )
    {
        var awardGenerationGroup = await db.Set<AwardGenerationGroup>()
            .SingleOrDefaultAsync(i => i.Id == id, cancellationToken);

        if (awardGenerationGroup == null)
        {
            return NotFound();
        }

        await using var txn = await db.Database.BeginTransactionAsync(cancellationToken);
        
        // clear out current awards
        awardGenerationGroup.Awards = [];
        awardGenerationGroup.ProjectAwards = [];
        await db.Set<Award>()
            .Where(g => g.GenerationGroupId == id)
            .ExecuteDeleteAsync(cancellationToken);
        await db.Set<ProjectAward>()
            .Where(g => g.GenerationGroupId == id)
            .ExecuteDeleteAsync(cancellationToken);
        
        awardGenerationGroup.UpdatedAt = DateTime.UtcNow;
        await GenerateAwards(awardGenerationGroup, awardGenerators, projectAwardCalculations, cancellationToken);
        
        await db.SaveChangesAsync(cancellationToken);
        await txn.CommitAsync(cancellationToken);
        
        return NoContent();
    }

    private static async Task GenerateAwards(
        AwardGenerationGroup group,
        IEnumerable<IAwardGenerator> awardGenerators,
        IEnumerable<IProjectAwardCalculation> projectAwardCalculations,
        CancellationToken cancellationToken
    )
    {
        foreach (var awardGenerator in awardGenerators)
        {
            var awards = await awardGenerator.GenerateAsync(group, cancellationToken);
            group.Awards!.AddRange(awards);
        }

        foreach (var calculation in projectAwardCalculations)
        {
            var projectAwardCounts = await calculation.GenerateAsync(group, cancellationToken);
            group.ProjectAwards!.AddRange(
                projectAwardCounts.Select(it => new ProjectAward
                {
                    GenerationGroup = group,
                    Heading = "WookieeProjects",
                    Type = calculation.Name,
                    Project = it.Project,
                    Count = it.Count
                })    
            );
        }
    }
}
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WookiepediaStatusArticleData.Database;
using WookiepediaStatusArticleData.Models.Awards;
using WookiepediaStatusArticleData.Nominations.Awards;
using WookiepediaStatusArticleData.Services.Awards;
using WookiepediaStatusArticleData.Services.Awards.NominatorAwardCalculations;
using WookiepediaStatusArticleData.Services.Awards.ProjectAwardCalculations;

namespace WookiepediaStatusArticleData.Controllers;

[Authorize]
[Route("award-generation-groups")]
public class AwardGenerationGroupsController(
    WookiepediaDbContext db,
    IEnumerable<INominatorAwardCalculation> awardGenerators,
    IEnumerable<IProjectAwardCalculation> projectAwardCalculations,
    NominatorAwardPlacementCalculation placementCalculation
) : Controller
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

        await using var txn = await db.Database.BeginTransactionAsync(cancellationToken);
        await GenerateAwards(newEntity, cancellationToken);
        db.Add(newEntity);
        // flush changes first so the rows are in the DB
        await db.SaveChangesAsync(cancellationToken);
        // then, run the fancy SQL to determine placement for each nominator
        await DeterminePlacement(newEntity, cancellationToken);
        // flush the changes again so the placement is updated
        await db.SaveChangesAsync(cancellationToken);
        // ... and THEN we're done!
        await txn.CommitAsync(cancellationToken);
        
        return RedirectToAction("Index");
    }

    [HttpPost("{id:int}")]
    public async Task<IActionResult> RefreshAwards(
        [FromRoute] int id,
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
        await GenerateAwards(awardGenerationGroup, cancellationToken);
        
        // flush changes first so the rows are in the DB
        await db.SaveChangesAsync(cancellationToken);
        // then, run the fancy SQL to determine placement for each nominator
        await DeterminePlacement(awardGenerationGroup, cancellationToken);
        // flush the changes again so the placement is updated
        await db.SaveChangesAsync(cancellationToken);
        // ... and THEN we're done!
        await txn.CommitAsync(cancellationToken);
        
        return NoContent();
    }

    private async Task GenerateAwards(
        AwardGenerationGroup group,
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

    private async Task DeterminePlacement(AwardGenerationGroup group, CancellationToken cancellationToken)
    {
        var placementProjections = await placementCalculation.CalculatePlacementAsync(
            group.Id,
            3,
            cancellationToken
        );

        foreach (var placementProjection in placementProjections)
        {
            var award = group.Awards!.FirstOrDefault(it => it.Id == placementProjection.Id);

            if (award == null) continue;

            award.Placement = placementProjection.Rank switch
            {
                1 => AwardPlacement.First,
                2 => AwardPlacement.Second,
                3 => AwardPlacement.Third,
                _ => AwardPlacement.DidNotPlace
            };
        }
    }
}
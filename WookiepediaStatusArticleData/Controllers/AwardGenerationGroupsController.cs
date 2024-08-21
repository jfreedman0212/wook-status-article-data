using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WookiepediaStatusArticleData.Database;
using WookiepediaStatusArticleData.Models.Awards;
using WookiepediaStatusArticleData.Nominations.Awards;
using WookiepediaStatusArticleData.Services.Awards;

namespace WookiepediaStatusArticleData.Controllers;

[Authorize]
[ApiController]
[Route("award-generation-groups")]
public class AwardGenerationGroupsController(WookiepediaDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetListAsync(CancellationToken cancellationToken)
    {
        var list = await db.Set<AwardGenerationGroup>()
            .OrderByDescending(g => g.StartedAt)
            .ThenByDescending(g => g.EndedAt)
            .ThenBy(g => g.Name)
            .Select(g => new AwardGenerationGroupViewModel
            {
                Id = g.Id,
                Name = g.Name,
                StartedAt = g.StartedAt,
                EndedAt = g.EndedAt
            })
            .ToListAsync(cancellationToken);
        
        return Ok(list);
    }
    
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetByIdAsync(
        [FromRoute] int id,
        CancellationToken cancellationToken
    )
    {
        var group = await db.Set<AwardGenerationGroup>()
            .Where(it => it.Id == id)
            .Select(g => new AwardGenerationGroupViewModel
            {
                Id = g.Id,
                Name = g.Name,
                StartedAt = g.StartedAt,
                EndedAt = g.EndedAt
            })
            .SingleOrDefaultAsync(cancellationToken);
        
        if (group == null) return NotFound();
        
        return Ok(group);
    }

    [HttpPost]
    public async Task<IActionResult> CreateAsync(
        [FromBody] AwardGenerationGroupForm form,
        [FromServices] IEnumerable<IAwardGenerator> awardGenerators,
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
        
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        var newEntity = new AwardGenerationGroup
        {
            Name = form.Name,
            StartedAt = startedAt,
            EndedAt = endedAt,
            Awards = []
        };

        foreach (var awardGenerator in awardGenerators)
        {
            var awards = await awardGenerator.GenerateAsync(newEntity, cancellationToken);
            newEntity.Awards = newEntity.Awards.Concat(awards).ToArray();
        }

        db.Add(newEntity);
        await db.SaveChangesAsync(cancellationToken);
        
        return Ok(newEntity.Id);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteAsync(
        [FromRoute] int id,
        CancellationToken cancellationToken
    )
    {
        var group = await db.Set<AwardGenerationGroup>()
            .Include(it => it.Awards)
            .SingleOrDefaultAsync(it => it.Id == id, cancellationToken);
        
        if (group == null) return NoContent();

        db.Remove(group);
        await db.SaveChangesAsync(cancellationToken);
        
        return NoContent();
    }
}
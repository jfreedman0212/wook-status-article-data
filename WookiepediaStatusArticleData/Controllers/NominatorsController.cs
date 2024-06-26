using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WookiepediaStatusArticleData.Database;
using WookiepediaStatusArticleData.Models.Nominators;
using WookiepediaStatusArticleData.Nominations.Nominators;
using WookiepediaStatusArticleData.Services;
using WookiepediaStatusArticleData.Services.Nominators;

namespace WookiepediaStatusArticleData.Controllers;

[Authorize]
[ApiController]
[Route("nominators")]
public class NominatorsController(WookiepediaDbContext db) : ControllerBase
{
    [HttpGet]
    public async IAsyncEnumerable<NominatorViewModel> Index(
        [EnumeratorCancellation] CancellationToken cancellationToken
    )
    {
        var nominators = db.Set<Nominator>()
            .OrderBy(nominator => nominator.Name)
            .Include(nominator => nominator.Attributes!.Where(attr => attr.EffectiveEndAt == null))
            .AsAsyncEnumerable()
            .WithCancellation(cancellationToken);

        await foreach (var nominator in nominators)
        {
            yield return new NominatorViewModel
            {
                Id = nominator.Id,
                Name = nominator.Name,
                Attributes = nominator.Attributes!
                    .Select(attr => new NominatorAttributeViewModel
                    {
                        Id = attr.Id,
                        AttributeName = attr.AttributeName,
                        EffectiveAt = attr.EffectiveAt
                    })
                    .DistinctBy(attr => attr.AttributeName)
                    .ToList()
            };
        }
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get([FromRoute] int id, CancellationToken cancellationToken)
    {
        var nominator = await db.Set<Nominator>()
            .Include(nominator => nominator.Attributes!.Where(attr => attr.EffectiveEndAt == null))
            .SingleOrDefaultAsync(it => it.Id == id, cancellationToken);

        if (nominator == null) return NotFound();

        return Ok(new NominatorViewModel
        {
            Id = nominator.Id,
            Name = nominator.Name,
            Attributes = nominator.Attributes!
                .Select(attr => new NominatorAttributeViewModel
                {
                    Id = attr.Id,
                    AttributeName = attr.AttributeName,
                    EffectiveAt = attr.EffectiveAt
                })
                .DistinctBy(attr => attr.AttributeName)
                .ToList()
        });
    }

    [HttpPost("{id:int}")]
    public async Task<IActionResult> Edit(
        [FromRoute] int id,
        [FromBody] NominatorForm form,
        [FromServices] EditNominatorAction action,
        CancellationToken cancellationToken
    )
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        
        try
        {
            var nominator = await action.ExecuteAsync(id, form, cancellationToken);

            if (nominator == null) return NotFound();

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
        [FromBody] NominatorForm form,
        [FromServices] EditNominatorAction action,
        CancellationToken cancellationToken    
    )
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        
        try
        {
            await action.ExecuteAsync(null, form, cancellationToken);

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
}
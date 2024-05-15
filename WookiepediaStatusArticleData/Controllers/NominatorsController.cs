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
            .Select(nominator => new NominatorViewModel
            {
                Id = nominator.Id,
                Name = nominator.Name,
                Attributes = nominator.Attributes!
                    .Where(attr => attr.EffectiveEndAt == null)
                    .Select(attr => attr.AttributeName)
                    .Distinct()
                    .ToList()
            })
            .AsAsyncEnumerable()
            .WithCancellation(cancellationToken);

        await foreach (var nominator in nominators)
        {
            yield return nominator;
        }
    }
    
    [HttpPost("{id:int}/ban")]
    public async Task<IActionResult> BanNominator(
        [FromRoute] int id,
        [FromServices] BanNominatorAction action,
        CancellationToken cancellationToken
    )
    {
        var nominator = await action.BanAsync(id, cancellationToken);

        if (nominator == null) return NotFound();

        await db.SaveChangesAsync(cancellationToken);
        return NoContent(); // TODO ????
    }
    
    [HttpPost("{id:int}/un-ban")]
    public async Task<IActionResult> UnbanNominator(
        [FromRoute] int id,
        [FromServices] BanNominatorAction action,
        CancellationToken cancellationToken
    )
    {
        var nominator = await action.UnbanAsync(id, cancellationToken);
        
        if (nominator == null) return NotFound();
        
        await db.SaveChangesAsync(cancellationToken);
        return NoContent(); // TODO ???? 
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
        [FromServices] CreateNominatorAction action,
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
}
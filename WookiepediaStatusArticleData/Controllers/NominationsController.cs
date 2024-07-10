using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WookiepediaStatusArticleData.Database;
using WookiepediaStatusArticleData.Models.Nominations;
using WookiepediaStatusArticleData.Nominations.Nominations;
using WookiepediaStatusArticleData.Services;
using WookiepediaStatusArticleData.Services.Nominations;

namespace WookiepediaStatusArticleData.Controllers;

[Authorize]
[ApiController]
[Route("nominations")]
public class NominationsController(WookiepediaDbContext db) : ControllerBase
{
    [HttpGet]
    public async IAsyncEnumerable<NominationViewModel> Paginate(
        [FromQuery] NominationQuery query,
        [EnumeratorCancellation] CancellationToken cancellationToken
    )
    {
        var queryable = db.Set<Nomination>()
            .OrderByDescending(it => it.StartedAt)
            .ThenBy(it => it.Id)
            .Where(it => it.StartedAt < query.LastStartedAt || (it.StartedAt == query.LastStartedAt && it.Id > query.LastId))
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            queryable = queryable
                .Where(it => EF.Functions.ILike(it.ArticleName, query.Search + '%')
                    || it.Nominators!.Any(n => EF.Functions.ILike(n.Name, query.Search + '%'))
                    || it.Projects!.Any(n => EF.Functions.ILike(n.Name, query.Search + '%'))
                );
        }
        
        // TODO: add the number of total records that match the query here?
        
        var page = queryable
            .Take(query.PageSize)
            .Select(it => new NominationViewModel
            {
                Id = it.Id,
                ArticleName = it.ArticleName,
                Continuities = it.Continuities,
                Type = it.Type,
                Outcome = it.Outcome,
                StartedAt = it.StartedAt,
                EndedAt = it.EndedAt,
                StartWordCount = it.StartWordCount,
                EndWordCount = it.EndWordCount,
                Nominators = it.Nominators!
                    .OrderBy(n => n.Name)
                    .Select(n => new NominationNominatorViewModel
                    {
                        Id = n.Id,
                        Name = n.Name
                    })
                    .ToList(),
                Projects = it.Projects!
                    .OrderBy(n => n.Name)
                    .Select(n => new NominationProjectViewModel
                    {
                        Id = n.Id,
                        Name = n.Name,
                        Type = n.Type,
                        IsArchived = n.IsArchived
                    })
                    .ToList()
            })
            .AsAsyncEnumerable()
            .WithCancellation(cancellationToken);

        await foreach (var row in page)
        {
            yield return row;
        }
    }
    
    [HttpPost("upload")]
    public async Task<IActionResult> Upload(
        [FromServices] NominationImporter importer,
        CancellationToken cancellationToken    
    )
    {
        try
        {
            await importer.ExecuteAsync(Request.Body, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
            return NoContent();
        }
        catch (ValidationException ex)
        {
            foreach (var issue in ex.Issues)
            {
                ModelState.AddModelError("Upload", issue.Message);
            }
            
            return ValidationProblem(ModelState);
        }
    }
}
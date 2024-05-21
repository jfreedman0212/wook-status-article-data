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
        query.LastStartedAt ??= query.Order.Equals("desc", StringComparison.InvariantCultureIgnoreCase)
            ? new DateTime(DateOnly.MaxValue, TimeOnly.MaxValue, DateTimeKind.Utc)
            : new DateTime(DateOnly.MinValue, TimeOnly.MinValue, DateTimeKind.Utc);

        var queryable = query.Order.ToLower() switch
        {
            "desc" => db.Set<Nomination>()
                .OrderByDescending(it => it.StartedAt)
                .ThenBy(it => it.Id)
                .Where(it => it.StartedAt < query.LastStartedAt || (it.StartedAt == query.LastStartedAt && it.Id > query.LastId)),
            "asc" => db.Set<Nomination>()
                .OrderBy(it => it.StartedAt)
                .ThenBy(it => it.Id)
                .Where(it => it.StartedAt > query.LastStartedAt || (it.StartedAt == query.LastStartedAt && it.Id > query.LastId)),
            _ => throw new Exception($"Invalid value for order: {query.Order}. Expected either 'desc' or 'asc'.")
        };
        
        if (query.Continuity != null)
        {
            // continuities is an integer in the DB, but a list in C#. since the code below doesn't actually
            // run but is converted into a syntax tree to generate SQL, this works (although it's not pretty) 
            queryable = queryable.Where(it => ((int)(object)it.Continuities & (int)query.Continuity.Value) > 0);
        }
        
        if (query.Type != null)
        {
            queryable = queryable.Where(it => it.Type == query.Type.Value);
        }
        
        if (query.Outcome != null)
        {
            queryable = queryable.Where(it => it.Outcome == query.Outcome.Value);
        }
        
        DateTime? beginDateTime = query.StartedAt != null
            ? new DateTime(query.StartedAt.Value, TimeOnly.MinValue, DateTimeKind.Utc)
            : null;

        DateTime? endDateTime = query.EndedAt != null
            ? new DateTime(query.EndedAt.Value, TimeOnly.MaxValue, DateTimeKind.Utc)
            : null;

        if (beginDateTime != null)
        {
            queryable = queryable.Where(it => beginDateTime.Value <= it.StartedAt);
        }

        if (endDateTime != null)
        {
            queryable = queryable.Where(it => it.EndedAt <= endDateTime);
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
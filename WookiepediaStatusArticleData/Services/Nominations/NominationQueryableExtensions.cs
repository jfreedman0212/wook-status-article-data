using Microsoft.EntityFrameworkCore;
using WookiepediaStatusArticleData.Models.Nominations;
using WookiepediaStatusArticleData.Nominations.Nominations;

namespace WookiepediaStatusArticleData.Services.Nominations;

public static class NominationQueryableExtensions
{
    public static IQueryable<Nomination> Filter(this DbSet<Nomination> self, NominationQuery query)
    {
        var queryable = self.AsQueryable();
        
        if (query.Continuity != null)
        {
            queryable = queryable.WithContinuity(query.Continuity.Value);
        }
        
        if (query.Type != null)
        {
            queryable = queryable.WithType(query.Type.Value);
        }
        
        if (query.Outcome != null)
        {
            queryable = queryable.WithOutcome(query.Outcome.Value);
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

        if (query.ProjectId != null)
        {
            queryable = queryable.Where(it => it.Projects!.Any(p => p.Id == query.ProjectId));
        }
        
        if (query.NominatorId != null)
        {
            queryable = queryable.Where(it => it.Nominators!.Any(p => p.Id == query.NominatorId));
        }

        return queryable;
    }

    public static async Task<IList<NominationViewModel>> Paginate(
        this IQueryable<Nomination> self,
        NominationQuery query,
        CancellationToken cancellationToken
    )
    {
        query.LastStartedAt ??= query.Order.Equals("desc", StringComparison.InvariantCultureIgnoreCase)
            ? new DateTime(DateOnly.MaxValue, TimeOnly.MaxValue, DateTimeKind.Utc)
            : new DateTime(DateOnly.MinValue, TimeOnly.MinValue, DateTimeKind.Utc);
        
        var queryable = self;
        
        queryable = query.Order.ToLower() switch
        {
            "desc" => queryable
                .OrderByDescending(it => it.StartedAt)
                .ThenBy(it => it.Id)
                .Where(it => it.StartedAt < query.LastStartedAt || (it.StartedAt == query.LastStartedAt && it.Id > query.LastId)),
            "asc" => queryable
                .OrderBy(it => it.StartedAt)
                .ThenBy(it => it.Id)
                .Where(it => it.StartedAt > query.LastStartedAt || (it.StartedAt == query.LastStartedAt && it.Id > query.LastId)),
            _ => throw new Exception($"Invalid value for order: {query.Order}. Expected either 'desc' or 'asc'.")
        };
        
        return await queryable
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
            .ToListAsync(cancellationToken);
    }

    public static IQueryable<Nomination> WithOutcome(this IQueryable<Nomination> self, Outcome outcome)
    {
        return self.Where(it => it.Outcome == outcome);
    }
    
    public static IQueryable<Nomination> WithType(this IQueryable<Nomination> self, NominationType type)
    {
        return self.Where(it => it.Type == type);
    }
    
    public static IQueryable<Nomination> WithContinuity(this IQueryable<Nomination> self, Continuity continuity)
    {
        // continuities is an integer in the DB, but a list in C#. since the code below doesn't actually
        // run but is converted into a syntax tree to generate SQL, this works (although it's not pretty)
        return self.Where(it => ((int)(object)it.Continuities & (int)continuity) > 0);
    }
}
using Microsoft.EntityFrameworkCore;
using WookiepediaStatusArticleData.Models.Nominations;
using WookiepediaStatusArticleData.Nominations.Awards;
using WookiepediaStatusArticleData.Nominations.Nominations;
using WookiepediaStatusArticleData.Nominations.Nominators;
using WookiepediaStatusArticleData.Nominations.Projects;
using WookiepediaStatusArticleData.Services.Awards;

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

        if (query.StartedAt != null)
        {
            var date = new DateTime(query.StartedAt.Value, TimeOnly.MinValue, DateTimeKind.Utc);
            queryable = queryable.Where(it => it.StartedAt.Date == date);
        }

        if (query.EndedAt != null)
        {
            var date = new DateTime(query.EndedAt.Value, TimeOnly.MinValue, DateTimeKind.Utc);
            queryable = queryable.Where(it => it.EndedAt != null && it.EndedAt.Value.Date == date);
        }

        if (query.StartedBy != null)
        {
            var date = new DateTime(query.StartedBy.Value, TimeOnly.MinValue, DateTimeKind.Utc);
            queryable = queryable.Where(it => date <= it.StartedAt);
        }

        if (query.EndedBy != null)
        {
            var date = new DateTime(query.EndedBy.Value, TimeOnly.MaxValue, DateTimeKind.Utc);
            queryable = queryable.Where(it => it.EndedAt <= date);
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

    public static IQueryable<Nomination> WithNoWookieeProjects(this IQueryable<Nomination> self)
    {
        return self.Where(it => !it.Projects!.Any());
    }

    public static IQueryable<Nomination> WithAnyWookieeProject(this IQueryable<Nomination> self)
    {
        return self.Where(it => it.Projects!.Any());
    }

    public static IQueryable<Nomination> WithWookieeProject(this IQueryable<Nomination> self, Project project)
    {
        return self.Where(it => it.Projects!.Any(p => p.Id == project.Id));
    }

    public static IQueryable<Nomination> EndedWithinTimeframe(
        this IQueryable<Nomination> self,
        DateTime startedAt,
        DateTime endedAt
    )
    {
        // we only care about nominations that ENDED within the timeframe of this generation group
        return self.Where(it =>
            it.EndedAt != null
            && startedAt <= it.EndedAt
            && it.EndedAt <= endedAt
        );
    }

    public static IQueryable<Nomination> WithoutBannedNominators(this IQueryable<Nomination> self, DateTime createdAt)
    {
        return self.Include(nomination => nomination.Nominators!.Where(it => !it.Attributes!.Any(
            attr => attr.AttributeName == NominatorAttributeType.Banned
                    && attr.EffectiveAt <= createdAt
                    && (attr.EffectiveEndAt == null || createdAt <= attr.EffectiveEndAt)
        )));
    }

    public static IQueryable<Nomination> ForAwardCalculations(
        this IQueryable<Nomination> self,
        AwardGenerationGroup awardGenerationGroup
    )
    {
        return self.Include(n => n.Projects)
            .WithOutcome(Outcome.Successful)
            .EndedWithinTimeframe(awardGenerationGroup.StartedAt, awardGenerationGroup.EndedAt)
            .WithoutBannedNominators(awardGenerationGroup.CreatedAt);
    }

    public static IQueryable<NominatorNominationProjection> GroupByNominator(this IQueryable<Nomination> self)
    {
        return self.SelectMany(
            it => it.Nominators!,
            (nomination, nominator) => new NominatorNominationProjection
            {
                Nomination = nomination,
                Nominator = nominator
            }
        );
    }

    public static IQueryable<IGrouping<Project, Nomination>> GroupByProject(this IQueryable<Nomination> self)
    {
        return self.SelectMany(
            it => it.Projects!,
            (nomination, project) => new NominationProjectProjection
            {
                Nomination = nomination,
                Project = project
            }
        )
        .GroupBy(it => it.Project, it => it.Nomination);
    }
}

public class NominationProjectProjection
{
    public required Nomination Nomination { get; init; }
    public required Project Project { get; init; }
}
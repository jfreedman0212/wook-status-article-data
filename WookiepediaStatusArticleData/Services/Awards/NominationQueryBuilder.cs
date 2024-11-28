using Microsoft.EntityFrameworkCore;
using WookiepediaStatusArticleData.Database;
using WookiepediaStatusArticleData.Nominations.Awards;
using WookiepediaStatusArticleData.Nominations.Nominations;
using WookiepediaStatusArticleData.Nominations.Nominators;
using WookiepediaStatusArticleData.Services.Nominations;

namespace WookiepediaStatusArticleData.Services.Awards;

public class NominationQueryBuilder(WookiepediaDbContext db, AwardGenerationGroup awardGenerationGroup)
{
    private IQueryable<Nomination> _nominationsQuery = db.Set<Nomination>()
        // all nomination queries need to be in the specified time range AND successful.
        .WithinRange(awardGenerationGroup.StartedAt, awardGenerationGroup.EndedAt)
        .WithOutcome(Outcome.Successful);

    public NominationQueryBuilder WithType(NominationType nominationType)
    {
        _nominationsQuery = _nominationsQuery.WithType(nominationType);
        return this;
    }

    public NominationQueryBuilder WithContinuity(Continuity continuity)
    {
        _nominationsQuery = _nominationsQuery.WithContinuity(continuity);
        return this;
    }

    public NominationNominatorQueryBuilder WithNominatorAttribute(NominatorAttributeType attr1, NominatorAttributeType? attr2 = null)
    {
        var newBuilder = new NominationNominatorQueryBuilder(_nominationsQuery, DateTime.UtcNow);
        return newBuilder.WithNominatorAttribute(attr1, attr2);
    }

    public async Task<IList<Award>> BuildAsync(string type, CancellationToken cancellationToken)
    {
        var newBuilder = new NominationNominatorQueryBuilder(_nominationsQuery, DateTime.UtcNow);
        return await newBuilder.BuildAsync(type, cancellationToken);
    }
}

public class NominatorNominationProjection
{
    public required Nomination Nomination { get; init; }
    public required Nominator Nominator { get; init; }
}

public class NominationNominatorQueryBuilder
{
    private IQueryable<NominatorNominationProjection> _projectionsQuery;

    public NominationNominatorQueryBuilder(IQueryable<Nomination> nominationsQuery, DateTime now)
    {
        _projectionsQuery = nominationsQuery
            .SelectMany(
                it => it.Nominators!,
                (nomination, nominator) => new NominatorNominationProjection
                {
                    Nomination = nomination,
                    Nominator = nominator
                }
            )
            // if the nominator is banned at the time of generation, do not include them in the count
            .Where(it => !it.Nominator.Attributes!.Any(
                attr => attr.AttributeName == NominatorAttributeType.Banned
                        && attr.EffectiveAt <= now
                        && (attr.EffectiveEndAt == null || now <= attr.EffectiveEndAt)
            ));
    }
    
    public NominationNominatorQueryBuilder WithNominatorAttribute(NominatorAttributeType attr1, NominatorAttributeType? attr2 = null)
    {
        _projectionsQuery = _projectionsQuery.Where(it => it.Nominator.Attributes!.Any(
            attr => (attr.AttributeName == attr1 || attr.AttributeName == attr2)
                    // if the attribute overlaps at all with the nomination window, count it 
                    && (it.Nomination.EndedAt == null || attr.EffectiveAt <= it.Nomination.EndedAt)
                    && (attr.EffectiveEndAt == null || it.Nomination.StartedAt <= attr.EffectiveEndAt)
        ));
        return this;
    }

    public async Task<IList<Award>> BuildAsync(string type, CancellationToken cancellationToken)
    {
        var results = await _projectionsQuery
            .GroupBy(it => it.Nominator)
            .Select(it => new
            {
                Nominator = it.Key,
                Count = it.Count()
            })
            .ToListAsync(cancellationToken);
        
        return results
            .Select(it => new Award
            {
                Type = type,
                Nominator = it.Nominator,
                Count = it.Count
            })
            .ToList();
    }
}
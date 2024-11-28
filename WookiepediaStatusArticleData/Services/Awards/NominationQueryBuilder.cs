using Microsoft.EntityFrameworkCore;
using WookiepediaStatusArticleData.Database;
using WookiepediaStatusArticleData.Nominations.Awards;
using WookiepediaStatusArticleData.Nominations.Nominations;
using WookiepediaStatusArticleData.Nominations.Nominators;
using WookiepediaStatusArticleData.Services.Nominations;

namespace WookiepediaStatusArticleData.Services.Awards;

public interface IQueryBuilder
{
    Task<IList<Award>> BuildAsync(AwardGenerationGroup awardGenerationGroup, CancellationToken cancellationToken);
}

public class NominationQueryBuilder(string type, WookiepediaDbContext db) : IQueryBuilder
{
    internal string Type => type;
    
    internal IQueryable<Nomination> NominationsQuery = db.Set<Nomination>()
        // we only care about successful nominations
        .WithOutcome(Outcome.Successful);

    public NominationQueryBuilder WithType(NominationType nominationType)
    {
        NominationsQuery = NominationsQuery.WithType(nominationType);
        return this;
    }

    public NominationQueryBuilder WithContinuity(Continuity continuity)
    {
        NominationsQuery = NominationsQuery.WithContinuity(continuity);
        return this;
    }

    public NominationNominatorQueryBuilder WithNominatorAttribute(
        NominatorAttributeType attr1,
        NominatorAttributeType? attr2 = null
    )
    {
        var newBuilder = new NominationNominatorQueryBuilder(this);
        return newBuilder.WithNominatorAttribute(attr1, attr2);
    }

    public async Task<IList<Award>> BuildAsync(
        AwardGenerationGroup awardGenerationGroup,
        CancellationToken cancellationToken
    )
    {
        var newBuilder = new NominationNominatorQueryBuilder(this);
        return await newBuilder.BuildAsync(awardGenerationGroup, cancellationToken);
    }
}

internal class NominatorNominationProjection
{
    public required Nomination Nomination { get; init; }
    public required Nominator Nominator { get; init; }
}

public class NominationNominatorQueryBuilder : IQueryBuilder
{
    private IQueryable<NominatorNominationProjection> _projectionsQuery;
    private readonly NominationQueryBuilder _nominationQueryBuilder;

    internal NominationNominatorQueryBuilder(NominationQueryBuilder nominationQueryBuilder)
    {
        var now = DateTime.UtcNow;
        _nominationQueryBuilder = nominationQueryBuilder;
        _projectionsQuery = nominationQueryBuilder.NominationsQuery
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
    
    public NominationNominatorQueryBuilder WithNominatorAttribute(
        NominatorAttributeType attr1,
        NominatorAttributeType? attr2 = null
    )
    {
        _projectionsQuery = _projectionsQuery.Where(it => it.Nominator.Attributes!.Any(
            attr => (attr.AttributeName == attr1 || attr.AttributeName == attr2)
                    // if the attribute overlaps at all with the nomination window, count it 
                    && (it.Nomination.EndedAt == null || attr.EffectiveAt <= it.Nomination.EndedAt)
                    && (attr.EffectiveEndAt == null || it.Nomination.StartedAt <= attr.EffectiveEndAt)
        ));
        return this;
    }

    public async Task<IList<Award>> BuildAsync(
        AwardGenerationGroup awardGenerationGroup,
        CancellationToken cancellationToken
    )
    {
        var results = await _projectionsQuery
            // we only care about nominations that ENDED within the timeframe of this generation group 
            .Where(it => 
                it.Nomination.EndedAt != null 
                && awardGenerationGroup.StartedAt <= it.Nomination.EndedAt
                && it.Nomination.EndedAt <= awardGenerationGroup.EndedAt)
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
                Type = _nominationQueryBuilder.Type,
                Nominator = it.Nominator,
                Count = it.Count
            })
            .ToList();
    }
}
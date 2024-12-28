using Microsoft.EntityFrameworkCore;
using WookiepediaStatusArticleData.Database;
using WookiepediaStatusArticleData.Nominations.Awards;
using WookiepediaStatusArticleData.Nominations.Nominations;
using WookiepediaStatusArticleData.Nominations.Nominators;
using WookiepediaStatusArticleData.Nominations.Projects;
using WookiepediaStatusArticleData.Services.Nominations;

namespace WookiepediaStatusArticleData.Services.Awards;

public interface IQueryBuilder
{
    Task<IList<Award>> BuildAsync(AwardGenerationGroup awardGenerationGroup, CancellationToken cancellationToken);
}

public enum CountMode
{
    NumberOfArticles,
    NumberOfUniqueProjects,
    JocastaBotPoints,
}

public class NominationQueryBuilder : IQueryBuilder
{
    internal string Heading { get; }
    internal string Subheading { get; }
    internal string Type { get; }
    internal CountMode CountMode { get; private set; }

    internal IQueryable<Nomination> NominationsQuery { get; private set; }

    public NominationQueryBuilder(
        string heading,
        string subheading,
        string type,
        WookiepediaDbContext db
    )
    {
        Heading = heading;
        Subheading = subheading;
        Type = type;
        CountMode = CountMode.NumberOfArticles;
        NominationsQuery = db.Set<Nomination>()
            .Include(it => it.Projects)
            // we only care about successful nominations
            .WithOutcome(Outcome.Successful);
    }
    
    private NominationQueryBuilder(NominationQueryBuilder other)
    {
        Heading = other.Heading;
        Subheading = other.Subheading;
        Type = other.Type;
        NominationsQuery = other.NominationsQuery;
    }

    public NominationQueryBuilder WithCountMode(CountMode countMode)
    {
        CountMode = countMode;
        return this;
    }

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

    public NominationQueryBuilder WithNoWookieeProjects()
    {
        NominationsQuery = NominationsQuery.Where(it => !it.Projects!.Any());
        return this;
    }
    
    public NominationQueryBuilder WithAnyWookieeProject()
    {
        NominationsQuery = NominationsQuery.Where(it => it.Projects!.Any());
        return this;
    }
    
    public NominationQueryBuilder WithWookieeProject(Project project)
    {
        var newBuilder = new NominationQueryBuilder(this)
        {
            NominationsQuery = NominationsQuery.Where(it => it.Projects!.Any(p => p.Id == project.Id))
        };
        return newBuilder;
    }

    public NominationNominatorQueryBuilder WithPanelistsOnly()
    {
        var newBuilder = new NominationNominatorQueryBuilder(this);
        return newBuilder.WithPanelistsOnly();
    }

    public NominationNominatorQueryBuilder WithNonPanelistsOnly()
    {
        var newBuilder = new NominationNominatorQueryBuilder(this);
        return newBuilder.WithNonPanelistsOnly();
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

internal enum PanelistMode
{
    All,
    PanelistsOnly,
    NonPanelistsOnly
}

public class NominationNominatorQueryBuilder : IQueryBuilder
{
    private IQueryable<NominatorNominationProjection> _projectionsQuery;
    private readonly NominationQueryBuilder _nominationQueryBuilder;
    private PanelistMode _panelistMode;

    internal NominationNominatorQueryBuilder(NominationQueryBuilder nominationQueryBuilder)
    {
        _panelistMode = PanelistMode.All;
        _nominationQueryBuilder = nominationQueryBuilder;
        _projectionsQuery = nominationQueryBuilder.NominationsQuery
            .SelectMany(
                it => it.Nominators!,
                (nomination, nominator) => new NominatorNominationProjection
                {
                    Nomination = nomination,
                    Nominator = nominator
                }
            );
    }

    public NominationNominatorQueryBuilder WithPanelistsOnly()
    {
        _panelistMode = PanelistMode.PanelistsOnly;
        return this;
    }

    public NominationNominatorQueryBuilder WithNonPanelistsOnly()
    {
        _panelistMode = PanelistMode.NonPanelistsOnly;
        return this;
    }

    public async Task<IList<Award>> BuildAsync(
        AwardGenerationGroup awardGenerationGroup,
        CancellationToken cancellationToken
    )
    {
        var panelistsQuery = _panelistMode switch
        {
            PanelistMode.All => _projectionsQuery,
            PanelistMode.PanelistsOnly => _projectionsQuery.Where(it => it.Nominator.Attributes!.Any(
                attr => (attr.AttributeName == NominatorAttributeType.Inquisitor 
                         || attr.AttributeName == NominatorAttributeType.AcMember)
                        // if it overlaps with the year at all, treat them as if they have been a panelist the whole year
                        && attr.EffectiveAt <= awardGenerationGroup.EndedAt
                        && (attr.EffectiveEndAt == null || awardGenerationGroup.StartedAt <= attr.EffectiveEndAt)
            )),
            PanelistMode.NonPanelistsOnly => _projectionsQuery.Where(it =>
                !it.Nominator.Attributes!.Any(
                    attr => (attr.AttributeName == NominatorAttributeType.Inquisitor 
                             || attr.AttributeName == NominatorAttributeType.AcMember)
                            // if it overlaps with the year at all, treat them as if they have been a panelist the whole year
                            && attr.EffectiveAt <= awardGenerationGroup.EndedAt
                            && (attr.EffectiveEndAt == null || awardGenerationGroup.StartedAt <= attr.EffectiveEndAt)
                )
            ),
            _ => throw new ArgumentOutOfRangeException()
        };
        
        var groupingQuery = panelistsQuery
            // if the nominator is banned at the time of generation, do not include them in the count
            .Where(it => !it.Nominator.Attributes!.Any(
                attr => attr.AttributeName == NominatorAttributeType.Banned
                        && attr.EffectiveAt <= awardGenerationGroup.CreatedAt
                        && (attr.EffectiveEndAt == null || awardGenerationGroup.CreatedAt <= attr.EffectiveEndAt)
            ))
            // we only care about nominations that ENDED within the timeframe of this generation group 
            .Where(it =>
                it.Nomination.EndedAt != null
                && awardGenerationGroup.StartedAt <= it.Nomination.EndedAt
                && it.Nomination.EndedAt <= awardGenerationGroup.EndedAt)
            .GroupBy(it => it.Nominator);

        var query = _nominationQueryBuilder.CountMode switch
        {
            CountMode.NumberOfArticles => groupingQuery.Select(it => new
            {
                Nominator = it.Key,
                Count = it.Count()
            }),
            CountMode.NumberOfUniqueProjects => groupingQuery.Select(it => new
            {
                Nominator = it.Key,
                Count = it.SelectMany(p => p.Nomination.Projects!).Distinct().Count()
            }),
            CountMode.JocastaBotPoints => groupingQuery.Select(it => new
            {
                Nominator = it.Key,
                // can't do a switch expression to generate a case expression. this is the next best thing,
                // but it's ugly :(
                Count = it.Sum(p => 
                    p.Nomination.Type == NominationType.Comprehensive ? 1 :
                    p.Nomination.Type == NominationType.Good ? 3 :
                    p.Nomination.Type == NominationType.Featured ? 5 :
                    0)
            }),
            _ => throw new ArgumentOutOfRangeException(
                nameof(_nominationQueryBuilder.CountMode),
                "Count Mode must be NumberOfArticles, NumberOfUniqueProjects, or JocastaBotPoints"
            )
        };
        
        var results = await query.ToListAsync(cancellationToken);

        return results
            .Select(it => new Award
            {
                Heading = _nominationQueryBuilder.Heading,
                Subheading = _nominationQueryBuilder.Subheading,
                Type = _nominationQueryBuilder.Type,
                Nominator = it.Nominator,
                Count = it.Count
            })
            .ToList();
    }
}
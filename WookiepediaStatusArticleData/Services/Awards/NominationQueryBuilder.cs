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
        NominationsQuery = NominationsQuery.WithNoWookieeProjects();
        return this;
    }
    
    public NominationQueryBuilder WithAnyWookieeProject()
    {
        NominationsQuery = NominationsQuery.WithAnyWookieeProject();
        return this;
    }
    
    public NominationQueryBuilder WithWookieeProject(Project project)
    {
        var newBuilder = new NominationQueryBuilder(this)
        {
            NominationsQuery = NominationsQuery.WithWookieeProject(project)
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

public class NominatorNominationProjection
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
    private readonly IQueryable<NominatorNominationProjection> _projectionsQuery;
    private readonly NominationQueryBuilder _nominationQueryBuilder;
    private PanelistMode _panelistMode;

    internal NominationNominatorQueryBuilder(NominationQueryBuilder nominationQueryBuilder)
    {
        _panelistMode = PanelistMode.All;
        _nominationQueryBuilder = nominationQueryBuilder;
        _projectionsQuery = nominationQueryBuilder.NominationsQuery.GroupByNominator();
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
            PanelistMode.PanelistsOnly => _projectionsQuery.WithPanelistsOnly(
                awardGenerationGroup.StartedAt,
                awardGenerationGroup.EndedAt
            ),
            PanelistMode.NonPanelistsOnly => _projectionsQuery.WithNonPanelistsOnly(
                awardGenerationGroup.StartedAt,
                awardGenerationGroup.EndedAt
            ),
            _ => throw new ArgumentOutOfRangeException()
        };
        
        var groupingQuery = panelistsQuery
            .WithoutBannedNominators(awardGenerationGroup.CreatedAt)
            .EndedWithinTimeframe(awardGenerationGroup.StartedAt, awardGenerationGroup.EndedAt);

        var query = _nominationQueryBuilder.CountMode switch
        {
            CountMode.NumberOfArticles => groupingQuery.CountByNumberOfArticles(),
            CountMode.NumberOfUniqueProjects => groupingQuery.CountByNumberOfUniqueProjects(),
            CountMode.JocastaBotPoints => groupingQuery.CountByJocastaBotPoints(),
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
                Count = it.Count,
                // treat this as the default. will be updated in a subsequent step if necessary
                Placement = AwardPlacement.DidNotPlace
            })
            .ToList();
    }
}
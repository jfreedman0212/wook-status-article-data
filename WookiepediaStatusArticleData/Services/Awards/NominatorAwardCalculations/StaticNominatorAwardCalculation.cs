using WookiepediaStatusArticleData.Database;
using WookiepediaStatusArticleData.Nominations.Awards;
using WookiepediaStatusArticleData.Nominations.Nominations;

namespace WookiepediaStatusArticleData.Services.Awards.NominatorAwardCalculations;

public class StaticNominatorAwardCalculation(WookiepediaDbContext db) : INominatorAwardCalculation
{
    private readonly IList<IQueryBuilder> _awardQueryBuilders =
    [
        new NominationQueryBuilder(
                "Sheer Numbers",
                "All Nominators",
                "Most Comprehensive Articles",
                db
            )
            .WithType(NominationType.Comprehensive),

        new NominationQueryBuilder("Sheer Numbers", "Non-Panelist", "Overall", db)
            .WithNonPanelistsOnly(),
        new NominationQueryBuilder("Sheer Numbers", "Non-Panelist", "GA", db)
            .WithType(NominationType.Good)
            .WithNonPanelistsOnly(),
        new NominationQueryBuilder("Sheer Numbers", "Non-Panelist", "FA", db)
            .WithType(NominationType.Featured)
            .WithNonPanelistsOnly(),

        new NominationQueryBuilder("Sheer Numbers", "Panelist", "Overall", db)
            .WithPanelistsOnly(),
        new NominationQueryBuilder("Sheer Numbers", "Panelist", "GA", db)
            .WithType(NominationType.Good)
            .WithPanelistsOnly(),
        new NominationQueryBuilder("Sheer Numbers", "Panelist", "FA", db)
            .WithType(NominationType.Featured)
            .WithPanelistsOnly(),

        new NominationQueryBuilder("Highest Scores", "Non-Panelist", "", db)
            .WithCountMode(CountMode.JocastaBotPoints)
            .WithNonPanelistsOnly(),
        new NominationQueryBuilder("Highest Scores", "Panelist", "", db)
            .WithCountMode(CountMode.JocastaBotPoints)
            .WithPanelistsOnly(),
        
        new NominationQueryBuilder("Continuity", "Comprehensive", "Canon", db)
            .WithType(NominationType.Comprehensive)
            .WithContinuity(Continuity.Canon),
        new NominationQueryBuilder("Continuity", "Comprehensive", "Legends", db)
            .WithType(NominationType.Comprehensive)
            .WithContinuity(Continuity.Legends),
        new NominationQueryBuilder("Continuity", "Comprehensive", "OOU", db)
            .WithType(NominationType.Comprehensive)
            .WithContinuity(Continuity.OutOfUniverse),

        new NominationQueryBuilder("Continuity", "Canon", "Non-Panelist Overall", db)
            .WithContinuity(Continuity.Canon)
            .WithNonPanelistsOnly(),
        new NominationQueryBuilder("Continuity", "Canon", "Non-Panelist GA", db)
            .WithType(NominationType.Good)
            .WithContinuity(Continuity.Canon)
            .WithNonPanelistsOnly(),
        new NominationQueryBuilder("Continuity", "Canon", "Non-Panelist FA", db)
            .WithType(NominationType.Featured)
            .WithContinuity(Continuity.Canon)
            .WithNonPanelistsOnly(),

        new NominationQueryBuilder("Continuity", "Canon", "Panelist Overall", db)
            .WithContinuity(Continuity.Canon)
            .WithPanelistsOnly(),
        new NominationQueryBuilder("Continuity", "Canon", "Panelist GA", db)
            .WithType(NominationType.Good)
            .WithContinuity(Continuity.Canon)
            .WithPanelistsOnly(),
        new NominationQueryBuilder("Continuity", "Canon", "Panelist FA", db)
            .WithType(NominationType.Featured)
            .WithContinuity(Continuity.Canon)
            .WithPanelistsOnly(),

        new NominationQueryBuilder("Continuity", "Legends", "Non-Panelist Overall", db)
            .WithContinuity(Continuity.Legends)
            .WithNonPanelistsOnly(),
        new NominationQueryBuilder("Continuity", "Legends", "Non-Panelist GA", db)
            .WithType(NominationType.Good)
            .WithContinuity(Continuity.Legends)
            .WithNonPanelistsOnly(),
        new NominationQueryBuilder("Continuity", "Legends", "Non-Panelist FA", db)
            .WithType(NominationType.Featured)
            .WithContinuity(Continuity.Legends)
            .WithNonPanelistsOnly(),

        new NominationQueryBuilder("Continuity", "Legends", "Panelist Overall", db)
            .WithContinuity(Continuity.Legends)
            .WithPanelistsOnly(),
        new NominationQueryBuilder("Continuity", "Legends", "Panelist GA", db)
            .WithType(NominationType.Good)
            .WithContinuity(Continuity.Legends)
            .WithPanelistsOnly(),
        new NominationQueryBuilder("Continuity", "Legends", "Panelist FA", db)
            .WithType(NominationType.Featured)
            .WithContinuity(Continuity.Legends)
            .WithPanelistsOnly(),

        new NominationQueryBuilder("Continuity", "OOU", "Non-Panelist Overall", db)
            .WithContinuity(Continuity.OutOfUniverse)
            .WithNonPanelistsOnly(),
        new NominationQueryBuilder("Continuity", "OOU", "Non-Panelist GA", db)
            .WithType(NominationType.Good)
            .WithContinuity(Continuity.OutOfUniverse)
            .WithNonPanelistsOnly(),
        new NominationQueryBuilder("Continuity", "OOU", "Non-Panelist FA", db)
            .WithType(NominationType.Featured)
            .WithContinuity(Continuity.OutOfUniverse)
            .WithNonPanelistsOnly(),

        new NominationQueryBuilder("Continuity", "OOU", "Panelist Overall", db)
            .WithContinuity(Continuity.OutOfUniverse)
            .WithPanelistsOnly(),
        new NominationQueryBuilder("Continuity", "OOU", "Panelist GA", db)
            .WithType(NominationType.Good)
            .WithContinuity(Continuity.OutOfUniverse)
            .WithPanelistsOnly(),
        new NominationQueryBuilder("Continuity", "OOU", "Panelist FA", db)
            .WithType(NominationType.Featured)
            .WithContinuity(Continuity.OutOfUniverse)
            .WithPanelistsOnly(),

        new NominationQueryBuilder(
                "Supporting WookieeProjects... or not",
                "Affiliation with WookieeProjects", 
                "Comprehensive",
                db
            )
            .WithCountMode(CountMode.NumberOfUniqueProjects)
            .WithType(NominationType.Comprehensive)
            .WithAnyWookieeProject(),

        new NominationQueryBuilder(
                "Supporting WookieeProjects... or not",
                "Affiliation with WookieeProjects",
                "Non-Panelist Overall",
                db
            )
            .WithCountMode(CountMode.NumberOfUniqueProjects)
            .WithAnyWookieeProject()
            .WithNonPanelistsOnly(),
        new NominationQueryBuilder(
                "Supporting WookieeProjects... or not",
                "Affiliation with WookieeProjects",
                "Non-Panelist GA",
                db
            )
            .WithCountMode(CountMode.NumberOfUniqueProjects)
            .WithType(NominationType.Good)
            .WithAnyWookieeProject()
            .WithNonPanelistsOnly(),
        new NominationQueryBuilder(
                "Supporting WookieeProjects... or not",
                "Affiliation with WookieeProjects",
                "Non-Panelist FA",
                db
            )
            .WithCountMode(CountMode.NumberOfUniqueProjects)
            .WithType(NominationType.Featured)
            .WithAnyWookieeProject()
            .WithNonPanelistsOnly(),

        new NominationQueryBuilder(
                "Supporting WookieeProjects... or not",
                "Affiliation with WookieeProjects",
                "Panelist Overall",
                db
            )
            .WithCountMode(CountMode.NumberOfUniqueProjects)
            .WithAnyWookieeProject()
            .WithPanelistsOnly(),
        new NominationQueryBuilder(
                "Supporting WookieeProjects... or not",
                "Affiliation with WookieeProjects",
                "Panelist GA",
                db
            )
            .WithCountMode(CountMode.NumberOfUniqueProjects)
            .WithType(NominationType.Good)
            .WithAnyWookieeProject()
            .WithPanelistsOnly(),
        new NominationQueryBuilder(
                "Supporting WookieeProjects... or not",
                "Affiliation with WookieeProjects",
                "Panelist FA",
                db
            )
            .WithCountMode(CountMode.NumberOfUniqueProjects)
            .WithType(NominationType.Featured)
            .WithAnyWookieeProject()
            .WithPanelistsOnly(),

        new NominationQueryBuilder("Supporting WookieeProjects... or not", "Independence from WookieeProjects", "Comprehensive", db)
            .WithType(NominationType.Comprehensive)
            .WithNoWookieeProjects(),

        new NominationQueryBuilder("Supporting WookieeProjects... or not", "Independence from WookieeProjects", "Non-Panelist Overall", db)
            .WithNoWookieeProjects()
            .WithNonPanelistsOnly(),
        new NominationQueryBuilder("Supporting WookieeProjects... or not", "Independence from WookieeProjects", "Non-Panelist GA", db)
            .WithType(NominationType.Good)
            .WithNoWookieeProjects()
            .WithNonPanelistsOnly(),
        new NominationQueryBuilder("Supporting WookieeProjects... or not", "Independence from WookieeProjects", "Non-Panelist FA", db)
            .WithType(NominationType.Featured)
            .WithNoWookieeProjects()
            .WithNonPanelistsOnly(),

        new NominationQueryBuilder("Supporting WookieeProjects... or not", "Independence from WookieeProjects", "Panelist Overall", db)
            .WithNoWookieeProjects()
            .WithPanelistsOnly(),
        new NominationQueryBuilder("Supporting WookieeProjects... or not", "Independence from WookieeProjects", "Panelist GA", db)
            .WithType(NominationType.Good)
            .WithNoWookieeProjects()
            .WithPanelistsOnly(),
        new NominationQueryBuilder("Supporting WookieeProjects... or not", "Independence from WookieeProjects", "Panelist FA", db)
            .WithType(NominationType.Featured)
            .WithNoWookieeProjects()
            .WithPanelistsOnly(),
    ];

    public async Task<IEnumerable<Award>> GenerateAsync(AwardGenerationGroup generationGroup,
        CancellationToken cancellationToken)
    {
        var list = new List<Award>();

        foreach (var queryBuilder in _awardQueryBuilders)
        {
            list.AddRange(await queryBuilder.BuildAsync(generationGroup, cancellationToken));
        }

        return list;
    }
}
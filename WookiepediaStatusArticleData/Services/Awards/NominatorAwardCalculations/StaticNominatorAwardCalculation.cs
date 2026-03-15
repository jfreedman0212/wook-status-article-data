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
                db,
                "statusCA"
            )
            .WithType(NominationType.Comprehensive),

        new NominationQueryBuilder("Sheer Numbers", "Non-Panelist", "All Articles", db, "statusOverall")
            .WithNonPanelistsOnly(),
        new NominationQueryBuilder("Sheer Numbers", "Non-Panelist", $"{NominationType.Good.GetDisplayName()} Articles", db, "statusGA")
            .WithType(NominationType.Good)
            .WithNonPanelistsOnly(),
        new NominationQueryBuilder("Sheer Numbers", "Non-Panelist", $"{NominationType.Featured.GetDisplayName()} Articles", db, "statusFA")
            .WithType(NominationType.Featured)
            .WithNonPanelistsOnly(),

        new NominationQueryBuilder("Sheer Numbers", "Panelist", "All Articles", db, "pStatusOverall")
            .WithPanelistsOnly(),
        new NominationQueryBuilder("Sheer Numbers", "Panelist", $"{NominationType.Good.GetDisplayName()} Articles", db, "pStatusGA")
            .WithType(NominationType.Good)
            .WithPanelistsOnly(),
        new NominationQueryBuilder("Sheer Numbers", "Panelist", $"{NominationType.Featured.GetDisplayName()} Articles", db, "pStatusFA")
            .WithType(NominationType.Featured)
            .WithPanelistsOnly(),

        new NominationQueryBuilder("Highest Scores", "Non-Panelist", "", db, "HS")
            .WithCountMode(CountMode.JocastaBotPoints)
            .WithNonPanelistsOnly(),
        new NominationQueryBuilder("Highest Scores", "Panelist", "", db, "pHS")
            .WithCountMode(CountMode.JocastaBotPoints)
            .WithPanelistsOnly(),

        new NominationQueryBuilder("Continuity", "Comprehensive", "Canon", db, "canonCA")
            .WithType(NominationType.Comprehensive)
            .WithContinuity(Continuity.Canon),
        new NominationQueryBuilder("Continuity", "Comprehensive", "Legends", db, "legendsCA")
            .WithType(NominationType.Comprehensive)
            .WithContinuity(Continuity.Legends),
        new NominationQueryBuilder("Continuity", "Comprehensive", "OOU", db, "OOUCA")
            .WithType(NominationType.Comprehensive)
            .WithContinuity(Continuity.OutOfUniverse),

        new NominationQueryBuilder("Continuity", "Canon", "All Articles by Non-Panelists", db, "canonOverall")
            .WithContinuity(Continuity.Canon)
            .WithNonPanelistsOnly(),
        new NominationQueryBuilder("Continuity", "Canon", $"{NominationType.Good.GetDisplayName()} Articles by Non-Panelists", db, "canonGA")
            .WithType(NominationType.Good)
            .WithContinuity(Continuity.Canon)
            .WithNonPanelistsOnly(),
        new NominationQueryBuilder("Continuity", "Canon", $"{NominationType.Featured.GetDisplayName()} Articles by Non-Panelists", db, "canonFA")
            .WithType(NominationType.Featured)
            .WithContinuity(Continuity.Canon)
            .WithNonPanelistsOnly(),

        new NominationQueryBuilder("Continuity", "Canon", "All Articles by Panelists", db, "pCanonOverall")
            .WithContinuity(Continuity.Canon)
            .WithPanelistsOnly(),
        new NominationQueryBuilder("Continuity", "Canon", $"{NominationType.Good.GetDisplayName()} Articles by Panelists", db, "pCanonGA")
            .WithType(NominationType.Good)
            .WithContinuity(Continuity.Canon)
            .WithPanelistsOnly(),
        new NominationQueryBuilder("Continuity", "Canon", $"{NominationType.Featured.GetDisplayName()} Articles by Panelists", db, "pCanonFA")
            .WithType(NominationType.Featured)
            .WithContinuity(Continuity.Canon)
            .WithPanelistsOnly(),

        new NominationQueryBuilder("Continuity", "Legends", "All Articles by Non-Panelists", db, "legendsOverall")
            .WithContinuity(Continuity.Legends)
            .WithNonPanelistsOnly(),
        new NominationQueryBuilder("Continuity", "Legends", $"{NominationType.Good.GetDisplayName()} Articles by Non-Panelists", db, "legendsGA")
            .WithType(NominationType.Good)
            .WithContinuity(Continuity.Legends)
            .WithNonPanelistsOnly(),
        new NominationQueryBuilder("Continuity", "Legends", $"{NominationType.Featured.GetDisplayName()} Articles by Non-Panelists", db, "legendsFA")
            .WithType(NominationType.Featured)
            .WithContinuity(Continuity.Legends)
            .WithNonPanelistsOnly(),

        new NominationQueryBuilder("Continuity", "Legends", "All Articles by Panelists", db, "pLegendsOverall")
            .WithContinuity(Continuity.Legends)
            .WithPanelistsOnly(),
        new NominationQueryBuilder("Continuity", "Legends", $"{NominationType.Good.GetDisplayName()} Articles by Panelists", db, "pLegendsGA")
            .WithType(NominationType.Good)
            .WithContinuity(Continuity.Legends)
            .WithPanelistsOnly(),
        new NominationQueryBuilder("Continuity", "Legends", $"{NominationType.Featured.GetDisplayName()} Articles by Panelists", db, "pLegendsFA")
            .WithType(NominationType.Featured)
            .WithContinuity(Continuity.Legends)
            .WithPanelistsOnly(),

        new NominationQueryBuilder("Continuity", "OOU", "All Articles by Non-Panelists", db, "OOUOverall")
            .WithContinuity(Continuity.OutOfUniverse)
            .WithNonPanelistsOnly(),
        new NominationQueryBuilder("Continuity", "OOU", $"{NominationType.Good.GetDisplayName()} Articles by Non-Panelists", db, "OOUGA")
            .WithType(NominationType.Good)
            .WithContinuity(Continuity.OutOfUniverse)
            .WithNonPanelistsOnly(),
        new NominationQueryBuilder("Continuity", "OOU", $"{NominationType.Featured.GetDisplayName()} Articles by Non-Panelists", db, "OOUFA")
            .WithType(NominationType.Featured)
            .WithContinuity(Continuity.OutOfUniverse)
            .WithNonPanelistsOnly(),

        new NominationQueryBuilder("Continuity", "OOU", "All Articles by Panelists", db, "pOOUOverall")
            .WithContinuity(Continuity.OutOfUniverse)
            .WithPanelistsOnly(),
        new NominationQueryBuilder("Continuity", "OOU", $"{NominationType.Good.GetDisplayName()} Articles by Panelists", db, "pOOUGA")
            .WithType(NominationType.Good)
            .WithContinuity(Continuity.OutOfUniverse)
            .WithPanelistsOnly(),
        new NominationQueryBuilder("Continuity", "OOU", $"{NominationType.Featured.GetDisplayName()} Articles by Panelists", db, "pOOUFA")
            .WithType(NominationType.Featured)
            .WithContinuity(Continuity.OutOfUniverse)
            .WithPanelistsOnly(),

        new NominationQueryBuilder(
                "Supporting WookieeProjects... or not",
                "Affiliation with WookieeProjects",
                "Comprehensive",
                db,
                "mostWPCA"
            )
            .WithCountMode(CountMode.NumberOfUniqueProjects)
            .WithType(NominationType.Comprehensive)
            .WithAnyWookieeProject(),

        new NominationQueryBuilder(
                "Supporting WookieeProjects... or not",
                "Affiliation with WookieeProjects",
                "All Articles by Non-Panelists",
                db,
                "mostWP"
            )
            .WithCountMode(CountMode.NumberOfUniqueProjects)
            .WithAnyWookieeProject()
            .WithNonPanelistsOnly(),
        new NominationQueryBuilder(
                "Supporting WookieeProjects... or not",
                "Affiliation with WookieeProjects",
                $"{NominationType.Good.GetDisplayName()} Articles by Non-Panelists",
                db,
                "mostWPGA"
            )
            .WithCountMode(CountMode.NumberOfUniqueProjects)
            .WithType(NominationType.Good)
            .WithAnyWookieeProject()
            .WithNonPanelistsOnly(),
        new NominationQueryBuilder(
                "Supporting WookieeProjects... or not",
                "Affiliation with WookieeProjects",
                $"{NominationType.Featured.GetDisplayName()} Articles by Non-Panelists",
                db,
                "mostWPFA"
            )
            .WithCountMode(CountMode.NumberOfUniqueProjects)
            .WithType(NominationType.Featured)
            .WithAnyWookieeProject()
            .WithNonPanelistsOnly(),

        new NominationQueryBuilder(
                "Supporting WookieeProjects... or not",
                "Affiliation with WookieeProjects",
                "All Articles by Panelists",
                db,
                "pMostWP"
            )
            .WithCountMode(CountMode.NumberOfUniqueProjects)
            .WithAnyWookieeProject()
            .WithPanelistsOnly(),
        new NominationQueryBuilder(
                "Supporting WookieeProjects... or not",
                "Affiliation with WookieeProjects",
                $"{NominationType.Good.GetDisplayName()} Articles by Panelists",
                db,
                "pMostWPGA"
            )
            .WithCountMode(CountMode.NumberOfUniqueProjects)
            .WithType(NominationType.Good)
            .WithAnyWookieeProject()
            .WithPanelistsOnly(),
        new NominationQueryBuilder(
                "Supporting WookieeProjects... or not",
                "Affiliation with WookieeProjects",
                $"{NominationType.Featured.GetDisplayName()} Articles by Panelists",
                db,
                "pMostWPFA"
            )
            .WithCountMode(CountMode.NumberOfUniqueProjects)
            .WithType(NominationType.Featured)
            .WithAnyWookieeProject()
            .WithPanelistsOnly(),

        new NominationQueryBuilder("Supporting WookieeProjects... or not", "Independence from WookieeProjects", "Comprehensive", db, "nonWPCA")
            .WithType(NominationType.Comprehensive)
            .WithNoWookieeProjects(),

        new NominationQueryBuilder("Supporting WookieeProjects... or not", "Independence from WookieeProjects", "All Articles by Non-Panelists", db, "nonWPOverall")
            .WithNoWookieeProjects()
            .WithNonPanelistsOnly(),
        new NominationQueryBuilder("Supporting WookieeProjects... or not", "Independence from WookieeProjects", $"{NominationType.Good.GetDisplayName()} Articles by Non-Panelists", db, "nonWPGA")
            .WithType(NominationType.Good)
            .WithNoWookieeProjects()
            .WithNonPanelistsOnly(),
        new NominationQueryBuilder("Supporting WookieeProjects... or not", "Independence from WookieeProjects", $"{NominationType.Featured.GetDisplayName()} Articles by Non-Panelists", db, "nonWPFA")
            .WithType(NominationType.Featured)
            .WithNoWookieeProjects()
            .WithNonPanelistsOnly(),

        new NominationQueryBuilder("Supporting WookieeProjects... or not", "Independence from WookieeProjects", "All Articles by Panelists", db, "pNonWPOverall")
            .WithNoWookieeProjects()
            .WithPanelistsOnly(),
        new NominationQueryBuilder("Supporting WookieeProjects... or not", "Independence from WookieeProjects", $"{NominationType.Good.GetDisplayName()} Articles by Panelists", db, "pNonWPGA")
            .WithType(NominationType.Good)
            .WithNoWookieeProjects()
            .WithPanelistsOnly(),
        new NominationQueryBuilder("Supporting WookieeProjects... or not", "Independence from WookieeProjects", $"{NominationType.Featured.GetDisplayName()} Articles by Panelists", db, "pNonWPFA")
            .WithType(NominationType.Featured)
            .WithNoWookieeProjects()
            .WithPanelistsOnly(),
    ];

    public async Task<IEnumerable<Award>> GenerateAsync(AwardGenerationGroup generationGroup, CancellationToken cancellationToken)
    {
        var list = new List<Award>();

        foreach (var queryBuilder in _awardQueryBuilders)
        {
            list.AddRange(await queryBuilder.BuildAsync(generationGroup, cancellationToken));
        }

        return list;
    }
}
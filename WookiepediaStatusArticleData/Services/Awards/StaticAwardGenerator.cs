using WookiepediaStatusArticleData.Database;
using WookiepediaStatusArticleData.Nominations.Awards;
using WookiepediaStatusArticleData.Nominations.Nominations;
using WookiepediaStatusArticleData.Nominations.Nominators;

namespace WookiepediaStatusArticleData.Services.Awards;

public class StaticAwardGenerator(WookiepediaDbContext db) : IAwardGenerator
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
            .WithoutNominatorAttribute(NominatorAttributeType.Inquisitor, NominatorAttributeType.AcMember),
        new NominationQueryBuilder("Sheer Numbers", "Non-Panelist", "GA", db)
            .WithType(NominationType.Good)
            .WithoutNominatorAttribute(NominatorAttributeType.Inquisitor, NominatorAttributeType.AcMember),
        new NominationQueryBuilder("Sheer Numbers", "Non-Panelist", "FA", db)
            .WithType(NominationType.Featured)
            .WithoutNominatorAttribute(NominatorAttributeType.Inquisitor, NominatorAttributeType.AcMember),

        new NominationQueryBuilder("Sheer Numbers", "Panelist", "Overall", db)
            .WithNominatorAttribute(NominatorAttributeType.Inquisitor, NominatorAttributeType.AcMember),
        new NominationQueryBuilder("Sheer Numbers", "Panelist", "GA", db)
            .WithType(NominationType.Good)
            .WithNominatorAttribute(NominatorAttributeType.Inquisitor, NominatorAttributeType.AcMember),
        new NominationQueryBuilder("Sheer Numbers", "Panelist", "FA", db)
            .WithType(NominationType.Featured)
            .WithNominatorAttribute(NominatorAttributeType.Inquisitor, NominatorAttributeType.AcMember),

        
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
            .WithoutNominatorAttribute(NominatorAttributeType.AcMember, NominatorAttributeType.Inquisitor),
        new NominationQueryBuilder("Continuity", "Canon", "Non-Panelist GA", db)
            .WithType(NominationType.Good)
            .WithContinuity(Continuity.Canon)
            .WithoutNominatorAttribute(NominatorAttributeType.AcMember, NominatorAttributeType.Inquisitor),
        new NominationQueryBuilder("Continuity", "Canon", "Non-Panelist FA", db)
            .WithType(NominationType.Featured)
            .WithContinuity(Continuity.Canon)
            .WithoutNominatorAttribute(NominatorAttributeType.AcMember, NominatorAttributeType.Inquisitor),

        new NominationQueryBuilder("Continuity", "Canon", "Panelist Overall", db)
            .WithContinuity(Continuity.Canon)
            .WithNominatorAttribute(NominatorAttributeType.AcMember, NominatorAttributeType.Inquisitor),
        new NominationQueryBuilder("Continuity", "Canon", "Panelist GA", db)
            .WithType(NominationType.Good)
            .WithContinuity(Continuity.Canon)
            .WithNominatorAttribute(NominatorAttributeType.AcMember, NominatorAttributeType.Inquisitor),
        new NominationQueryBuilder("Continuity", "Canon", "Panelist FA", db)
            .WithType(NominationType.Featured)
            .WithContinuity(Continuity.Canon)
            .WithNominatorAttribute(NominatorAttributeType.AcMember, NominatorAttributeType.Inquisitor),

        new NominationQueryBuilder("Continuity", "Legends", "Non-Panelist Overall", db)
            .WithContinuity(Continuity.Legends)
            .WithoutNominatorAttribute(NominatorAttributeType.AcMember, NominatorAttributeType.Inquisitor),
        new NominationQueryBuilder("Continuity", "Legends", "Non-Panelist GA", db)
            .WithType(NominationType.Good)
            .WithContinuity(Continuity.Legends)
            .WithoutNominatorAttribute(NominatorAttributeType.AcMember, NominatorAttributeType.Inquisitor),
        new NominationQueryBuilder("Continuity", "Legends", "Non-Panelist FA", db)
            .WithType(NominationType.Featured)
            .WithContinuity(Continuity.Legends)
            .WithoutNominatorAttribute(NominatorAttributeType.AcMember, NominatorAttributeType.Inquisitor),

        new NominationQueryBuilder("Continuity", "Legends", "Panelist Overall", db)
            .WithContinuity(Continuity.Legends)
            .WithNominatorAttribute(NominatorAttributeType.AcMember, NominatorAttributeType.Inquisitor),
        new NominationQueryBuilder("Continuity", "Legends", "Panelist GA", db)
            .WithType(NominationType.Good)
            .WithContinuity(Continuity.Legends)
            .WithNominatorAttribute(NominatorAttributeType.AcMember, NominatorAttributeType.Inquisitor),
        new NominationQueryBuilder("Continuity", "Legends", "Panelist FA", db)
            .WithType(NominationType.Featured)
            .WithContinuity(Continuity.Legends)
            .WithNominatorAttribute(NominatorAttributeType.AcMember, NominatorAttributeType.Inquisitor),

        new NominationQueryBuilder("Continuity", "OOU", "Non-Panelist Overall", db)
            .WithContinuity(Continuity.OutOfUniverse)
            .WithoutNominatorAttribute(NominatorAttributeType.AcMember, NominatorAttributeType.Inquisitor),
        new NominationQueryBuilder("Continuity", "OOU", "Non-Panelist GA", db)
            .WithType(NominationType.Good)
            .WithContinuity(Continuity.OutOfUniverse)
            .WithoutNominatorAttribute(NominatorAttributeType.AcMember, NominatorAttributeType.Inquisitor),
        new NominationQueryBuilder("Continuity", "OOU", "Non-Panelist FA", db)
            .WithType(NominationType.Featured)
            .WithContinuity(Continuity.OutOfUniverse)
            .WithoutNominatorAttribute(NominatorAttributeType.AcMember, NominatorAttributeType.Inquisitor),

        new NominationQueryBuilder("Continuity", "OOU", "Panelist Overall", db)
            .WithContinuity(Continuity.OutOfUniverse)
            .WithNominatorAttribute(NominatorAttributeType.AcMember, NominatorAttributeType.Inquisitor),
        new NominationQueryBuilder("Continuity", "OOU", "Panelist GA", db)
            .WithType(NominationType.Good)
            .WithContinuity(Continuity.OutOfUniverse)
            .WithNominatorAttribute(NominatorAttributeType.AcMember, NominatorAttributeType.Inquisitor),
        new NominationQueryBuilder("Continuity", "OOU", "Panelist FA", db)
            .WithType(NominationType.Featured)
            .WithContinuity(Continuity.OutOfUniverse)
            .WithNominatorAttribute(NominatorAttributeType.AcMember, NominatorAttributeType.Inquisitor),

        new NominationQueryBuilder(
                "Supporting WookieeProjects... or not",
                "Affiliation with WookieeProjects", 
                "Comprehensive",
                db,
                CountMode.NumberOfUniqueProjects
            )
            .WithType(NominationType.Comprehensive)
            .WithAnyWookieeProject(),

        new NominationQueryBuilder(
                "Supporting WookieeProjects... or not",
                "Affiliation with WookieeProjects",
                "Non-Panelist Overall",
                db,
                CountMode.NumberOfUniqueProjects
            )
            .WithAnyWookieeProject()
            .WithoutNominatorAttribute(NominatorAttributeType.AcMember, NominatorAttributeType.Inquisitor),
        new NominationQueryBuilder(
                "Supporting WookieeProjects... or not",
                "Affiliation with WookieeProjects",
                "Non-Panelist GA",
                db,
                CountMode.NumberOfUniqueProjects
            )
            .WithType(NominationType.Good)
            .WithAnyWookieeProject()
            .WithoutNominatorAttribute(NominatorAttributeType.AcMember, NominatorAttributeType.Inquisitor),
        new NominationQueryBuilder(
                "Supporting WookieeProjects... or not",
                "Affiliation with WookieeProjects",
                "Non-Panelist FA",
                db,
                CountMode.NumberOfUniqueProjects
            )
            .WithType(NominationType.Featured)
            .WithAnyWookieeProject()
            .WithoutNominatorAttribute(NominatorAttributeType.AcMember, NominatorAttributeType.Inquisitor),

        new NominationQueryBuilder(
                "Supporting WookieeProjects... or not",
                "Affiliation with WookieeProjects",
                "Panelist Overall",
                db,
                CountMode.NumberOfUniqueProjects
            )
            .WithAnyWookieeProject()
            .WithNominatorAttribute(NominatorAttributeType.AcMember, NominatorAttributeType.Inquisitor),
        new NominationQueryBuilder(
                "Supporting WookieeProjects... or not",
                "Affiliation with WookieeProjects",
                "Panelist GA",
                db,
                CountMode.NumberOfUniqueProjects
            )
            .WithType(NominationType.Good)
            .WithAnyWookieeProject()
            .WithNominatorAttribute(NominatorAttributeType.AcMember, NominatorAttributeType.Inquisitor),
        new NominationQueryBuilder(
                "Supporting WookieeProjects... or not",
                "Affiliation with WookieeProjects",
                "Panelist FA",
                db,
                CountMode.NumberOfUniqueProjects
            )
            .WithType(NominationType.Featured)
            .WithAnyWookieeProject()
            .WithNominatorAttribute(NominatorAttributeType.AcMember, NominatorAttributeType.Inquisitor),

        new NominationQueryBuilder("Supporting WookieeProjects... or not", "Independence from WookieeProjects", "Comprehensive", db)
            .WithType(NominationType.Comprehensive)
            .WithNoWookieeProjects(),

        new NominationQueryBuilder("Supporting WookieeProjects... or not", "Independence from WookieeProjects", "Non-Panelist Overall", db)
            .WithNoWookieeProjects()
            .WithoutNominatorAttribute(NominatorAttributeType.AcMember, NominatorAttributeType.Inquisitor),
        new NominationQueryBuilder("Supporting WookieeProjects... or not", "Independence from WookieeProjects", "Non-Panelist GA", db)
            .WithType(NominationType.Good)
            .WithNoWookieeProjects()
            .WithoutNominatorAttribute(NominatorAttributeType.AcMember, NominatorAttributeType.Inquisitor),
        new NominationQueryBuilder("Supporting WookieeProjects... or not", "Independence from WookieeProjects", "Non-Panelist FA", db)
            .WithType(NominationType.Featured)
            .WithNoWookieeProjects()
            .WithoutNominatorAttribute(NominatorAttributeType.AcMember, NominatorAttributeType.Inquisitor),

        new NominationQueryBuilder("Supporting WookieeProjects... or not", "Independence from WookieeProjects", "Panelist Overall", db)
            .WithNoWookieeProjects()
            .WithNominatorAttribute(NominatorAttributeType.AcMember, NominatorAttributeType.Inquisitor),
        new NominationQueryBuilder("Supporting WookieeProjects... or not", "Independence from WookieeProjects", "Panelist GA", db)
            .WithType(NominationType.Good)
            .WithNoWookieeProjects()
            .WithNominatorAttribute(NominatorAttributeType.AcMember, NominatorAttributeType.Inquisitor),
        new NominationQueryBuilder("Supporting WookieeProjects... or not", "Independence from WookieeProjects", "Panelist FA", db)
            .WithType(NominationType.Featured)
            .WithNoWookieeProjects()
            .WithNominatorAttribute(NominatorAttributeType.AcMember, NominatorAttributeType.Inquisitor),
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
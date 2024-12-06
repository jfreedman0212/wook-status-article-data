using WookiepediaStatusArticleData.Database;
using WookiepediaStatusArticleData.Nominations.Awards;
using WookiepediaStatusArticleData.Nominations.Nominations;
using WookiepediaStatusArticleData.Nominations.Nominators;

namespace WookiepediaStatusArticleData.Services.Awards;

public class StaticAwardGenerator(WookiepediaDbContext db) : IAwardGenerator
{
    private readonly IList<IQueryBuilder> _awardQueryBuilders = 
    [
        new NominationQueryBuilder("Sheer Numbers", db)
            .WithType(NominationType.Comprehensive),
        
        new NominationQueryBuilder("Non-Inq/AG Overall", db)
            .WithoutNominatorAttribute(NominatorAttributeType.Inquisitor, NominatorAttributeType.AcMember),
        new NominationQueryBuilder("Non-Inq/AG GA", db)
            .WithType(NominationType.Good)
            .WithoutNominatorAttribute(NominatorAttributeType.Inquisitor, NominatorAttributeType.AcMember),
        new NominationQueryBuilder("Non-Inq/AG FA", db)
            .WithType(NominationType.Featured)
            .WithoutNominatorAttribute(NominatorAttributeType.Inquisitor, NominatorAttributeType.AcMember),
        
        new NominationQueryBuilder("Inquisitorius and AgriCorp Overall", db)
            .WithNominatorAttribute(NominatorAttributeType.Inquisitor, NominatorAttributeType.AcMember),
        new NominationQueryBuilder("Inquisitorius and AgriCorp GA", db)
            .WithType(NominationType.Good)
            .WithNominatorAttribute(NominatorAttributeType.Inquisitor, NominatorAttributeType.AcMember),
        new NominationQueryBuilder("Inquisitorius and AgriCorp FA", db)
            .WithType(NominationType.Featured)
            .WithNominatorAttribute(NominatorAttributeType.Inquisitor, NominatorAttributeType.AcMember),
        
        new NominationQueryBuilder("Canon Comprehensive", db)
            .WithType(NominationType.Comprehensive)
            .WithContinuity(Continuity.Canon),
        new NominationQueryBuilder("Legends Comprehensive", db)
            .WithType(NominationType.Comprehensive)
            .WithContinuity(Continuity.Legends),
        new NominationQueryBuilder("OOU Comprehensive", db)
            .WithType(NominationType.Comprehensive)
            .WithContinuity(Continuity.OutOfUniverse),
        
        new NominationQueryBuilder("Non-Inq/AG Canon Overall", db)
            .WithContinuity(Continuity.Canon)
            .WithoutNominatorAttribute(NominatorAttributeType.AcMember, NominatorAttributeType.Inquisitor),
        new NominationQueryBuilder("Non-Inq/AG Canon GA", db)
            .WithType(NominationType.Good)
            .WithContinuity(Continuity.Canon)
            .WithoutNominatorAttribute(NominatorAttributeType.AcMember, NominatorAttributeType.Inquisitor),
        new NominationQueryBuilder("Non-Inq/AG Canon FA", db)
            .WithType(NominationType.Featured)
            .WithContinuity(Continuity.Canon)
            .WithoutNominatorAttribute(NominatorAttributeType.AcMember, NominatorAttributeType.Inquisitor),
        
        new NominationQueryBuilder("Inq/AG Canon Overall", db)
            .WithContinuity(Continuity.Canon)
            .WithNominatorAttribute(NominatorAttributeType.AcMember, NominatorAttributeType.Inquisitor),
        new NominationQueryBuilder("Inq/AG Canon GA", db)
            .WithType(NominationType.Good)
            .WithContinuity(Continuity.Canon)
            .WithNominatorAttribute(NominatorAttributeType.AcMember, NominatorAttributeType.Inquisitor),
        new NominationQueryBuilder("Inq/AG Canon FA", db)
            .WithType(NominationType.Featured)
            .WithContinuity(Continuity.Canon)
            .WithNominatorAttribute(NominatorAttributeType.AcMember, NominatorAttributeType.Inquisitor),
    
        new NominationQueryBuilder("Non-Inq/AG Legends Overall", db)
            .WithContinuity(Continuity.Legends)
            .WithoutNominatorAttribute(NominatorAttributeType.AcMember, NominatorAttributeType.Inquisitor),
        new NominationQueryBuilder("Non-Inq/AG Legends GA", db)
            .WithType(NominationType.Good)
            .WithContinuity(Continuity.Legends)
            .WithoutNominatorAttribute(NominatorAttributeType.AcMember, NominatorAttributeType.Inquisitor),
        new NominationQueryBuilder("Non-Inq/AG Legends FA", db)
            .WithType(NominationType.Featured)
            .WithContinuity(Continuity.Legends)
            .WithoutNominatorAttribute(NominatorAttributeType.AcMember, NominatorAttributeType.Inquisitor),
        
        new NominationQueryBuilder("Inq/AG Legends Overall", db)
            .WithContinuity(Continuity.Legends)
            .WithNominatorAttribute(NominatorAttributeType.AcMember, NominatorAttributeType.Inquisitor),
        new NominationQueryBuilder("Inq/AG Legends GA", db)
            .WithType(NominationType.Good)
            .WithContinuity(Continuity.Legends)
            .WithNominatorAttribute(NominatorAttributeType.AcMember, NominatorAttributeType.Inquisitor),
        new NominationQueryBuilder("Inq/AG Legends FA", db)
            .WithType(NominationType.Featured)
            .WithContinuity(Continuity.Legends)
            .WithNominatorAttribute(NominatorAttributeType.AcMember, NominatorAttributeType.Inquisitor),
        
        new NominationQueryBuilder("Non-Inq/AG OOU Overall", db)
            .WithContinuity(Continuity.OutOfUniverse)
            .WithoutNominatorAttribute(NominatorAttributeType.AcMember, NominatorAttributeType.Inquisitor),
        new NominationQueryBuilder("Non-Inq/AG OOU GA", db)
            .WithType(NominationType.Good)
            .WithContinuity(Continuity.OutOfUniverse)
            .WithoutNominatorAttribute(NominatorAttributeType.AcMember, NominatorAttributeType.Inquisitor),
        new NominationQueryBuilder("Non-Inq/AG OOU FA", db)
            .WithType(NominationType.Featured)
            .WithContinuity(Continuity.OutOfUniverse)
            .WithoutNominatorAttribute(NominatorAttributeType.AcMember, NominatorAttributeType.Inquisitor),
        
        new NominationQueryBuilder("Inq/AG OOU Overall", db)
            .WithContinuity(Continuity.OutOfUniverse)
            .WithNominatorAttribute(NominatorAttributeType.AcMember, NominatorAttributeType.Inquisitor),
        new NominationQueryBuilder("Inq/AG OOU GA", db)
            .WithType(NominationType.Good)
            .WithContinuity(Continuity.OutOfUniverse)
            .WithNominatorAttribute(NominatorAttributeType.AcMember, NominatorAttributeType.Inquisitor),
        new NominationQueryBuilder("Inq/AG OOU FA", db)
            .WithType(NominationType.Featured)
            .WithContinuity(Continuity.OutOfUniverse)
            .WithNominatorAttribute(NominatorAttributeType.AcMember, NominatorAttributeType.Inquisitor),
    ];
    
    public async Task<IList<Award>> GenerateAsync(AwardGenerationGroup generationGroup, CancellationToken cancellationToken)
    {
        var list = new List<Award>();

        foreach (var queryBuilder in _awardQueryBuilders)
        {
            list.AddRange(await queryBuilder.BuildAsync(generationGroup, cancellationToken));
        }

        return list;
    }
}
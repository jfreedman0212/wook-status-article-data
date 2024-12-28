using WookiepediaStatusArticleData.Database;
using WookiepediaStatusArticleData.Nominations.Awards;
using WookiepediaStatusArticleData.Nominations.Nominations;
using WookiepediaStatusArticleData.Nominations.Nominators;
using WookiepediaStatusArticleData.Nominations.Projects;

namespace WookiepediaStatusArticleData.Services.Awards;

public class ProjectsAwardGenerator(WookiepediaDbContext db) : IAwardGenerator
{
    public async Task<IEnumerable<Award>> GenerateAsync(AwardGenerationGroup generationGroup, CancellationToken cancellationToken)
    {
        var projects = db.Set<Project>()
            .Where(p => !p.IsArchived)
            .ToList();
        
        var awards = new List<Award>();
        
        foreach (var project in projects)
        {
            awards.AddRange(
                await new NominationQueryBuilder("WookieeProject Contributions", "Non-Panelist Overall", project.Name, db)
                    .WithWookieeProject(project)
                    .WithoutNominatorAttribute(NominatorAttributeType.Inquisitor, NominatorAttributeType.AcMember)
                    .BuildAsync(generationGroup, cancellationToken)
            );
            
            awards.AddRange(
                await new NominationQueryBuilder("WookieeProject Contributions", "Panelist Overall", project.Name, db)
                    .WithWookieeProject(project)
                    .WithNominatorAttribute(NominatorAttributeType.Inquisitor, NominatorAttributeType.AcMember)
                    .BuildAsync(generationGroup, cancellationToken)
            );
            
            awards.AddRange(
                await new NominationQueryBuilder("WookieeProject Contributions", "Comprehensive", project.Name, db)
                    .WithWookieeProject(project)
                    .WithType(NominationType.Comprehensive)
                    .BuildAsync(generationGroup, cancellationToken)
            );
            
            awards.AddRange(
                await new NominationQueryBuilder("WookieeProject Contributions", "Non-Panelist GAs", project.Name, db)
                    .WithWookieeProject(project)
                    .WithType(NominationType.Good)
                    .WithoutNominatorAttribute(NominatorAttributeType.Inquisitor, NominatorAttributeType.AcMember)
                    .BuildAsync(generationGroup, cancellationToken)
            );
            
            awards.AddRange(
                await new NominationQueryBuilder("WookieeProject Contributions", "Panelist GAs", project.Name, db)
                    .WithWookieeProject(project)
                    .WithType(NominationType.Good)
                    .WithNominatorAttribute(NominatorAttributeType.Inquisitor, NominatorAttributeType.AcMember)
                    .BuildAsync(generationGroup, cancellationToken)
            );
            
            awards.AddRange(
                await new NominationQueryBuilder("WookieeProject Contributions", "Non-Panelist FAs", project.Name, db)
                    .WithWookieeProject(project)
                    .WithType(NominationType.Featured)
                    .WithoutNominatorAttribute(NominatorAttributeType.Inquisitor, NominatorAttributeType.AcMember)
                    .BuildAsync(generationGroup, cancellationToken)
            );
            
            awards.AddRange(
                await new NominationQueryBuilder("WookieeProject Contributions", "Panelist FAs", project.Name, db)
                    .WithWookieeProject(project)
                    .WithType(NominationType.Featured)
                    .WithNominatorAttribute(NominatorAttributeType.Inquisitor, NominatorAttributeType.AcMember)
                    .BuildAsync(generationGroup, cancellationToken)
            );
            
            awards.AddRange(
                await new NominationQueryBuilder("WookieeProject Contributions", "Non-Panelist Score", project.Name, db)
                    .WithCountMode(CountMode.JocastaBotPoints)
                    .WithWookieeProject(project)
                    .WithoutNominatorAttribute(NominatorAttributeType.Inquisitor, NominatorAttributeType.AcMember)
                    .BuildAsync(generationGroup, cancellationToken)
            );
            
            awards.AddRange(
                await new NominationQueryBuilder("WookieeProject Contributions", "Panelist Score", project.Name, db)
                    .WithCountMode(CountMode.JocastaBotPoints)
                    .WithWookieeProject(project)
                    .WithNominatorAttribute(NominatorAttributeType.Inquisitor, NominatorAttributeType.AcMember)
                    .BuildAsync(generationGroup, cancellationToken)
            );
        }

        return awards;
    }
}
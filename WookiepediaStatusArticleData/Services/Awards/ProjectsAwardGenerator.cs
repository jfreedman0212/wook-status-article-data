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
                await new NominationQueryBuilder($"Non-Inq/Non AC {project.Name} Overall", db)
                    .WithWookieeProject(project)
                    .WithoutNominatorAttribute(NominatorAttributeType.Inquisitor, NominatorAttributeType.AcMember)
                    .BuildAsync(generationGroup, cancellationToken)
            );
            
            awards.AddRange(
                await new NominationQueryBuilder($"Inq/AC {project.Name} Overall", db)
                    .WithWookieeProject(project)
                    .WithNominatorAttribute(NominatorAttributeType.Inquisitor, NominatorAttributeType.AcMember)
                    .BuildAsync(generationGroup, cancellationToken)
            );
            
            awards.AddRange(
                await new NominationQueryBuilder($"{project.Name} CA", db)
                    .WithWookieeProject(project)
                    .WithType(NominationType.Comprehensive)
                    .BuildAsync(generationGroup, cancellationToken)
            );
            
            awards.AddRange(
                await new NominationQueryBuilder($"Non-Inq/Non AC {project.Name} GA", db)
                    .WithWookieeProject(project)
                    .WithType(NominationType.Good)
                    .WithoutNominatorAttribute(NominatorAttributeType.Inquisitor, NominatorAttributeType.AcMember)
                    .BuildAsync(generationGroup, cancellationToken)
            );
            
            awards.AddRange(
                await new NominationQueryBuilder($"Inq/AC {project.Name} GA", db)
                    .WithWookieeProject(project)
                    .WithType(NominationType.Good)
                    .WithNominatorAttribute(NominatorAttributeType.Inquisitor, NominatorAttributeType.AcMember)
                    .BuildAsync(generationGroup, cancellationToken)
            );
            
            awards.AddRange(
                await new NominationQueryBuilder($"Non-Inq/Non AC {project.Name} FA", db)
                    .WithWookieeProject(project)
                    .WithType(NominationType.Featured)
                    .WithoutNominatorAttribute(NominatorAttributeType.Inquisitor, NominatorAttributeType.AcMember)
                    .BuildAsync(generationGroup, cancellationToken)
            );
            
            awards.AddRange(
                await new NominationQueryBuilder($"Inq/AC {project.Name} FA", db)
                    .WithWookieeProject(project)
                    .WithType(NominationType.Featured)
                    .WithNominatorAttribute(NominatorAttributeType.Inquisitor, NominatorAttributeType.AcMember)
                    .BuildAsync(generationGroup, cancellationToken)
            );
        }

        return awards;
    }
}
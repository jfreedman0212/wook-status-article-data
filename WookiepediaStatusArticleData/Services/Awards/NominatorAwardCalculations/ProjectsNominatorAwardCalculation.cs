using WookiepediaStatusArticleData.Database;
using WookiepediaStatusArticleData.Nominations.Awards;
using WookiepediaStatusArticleData.Nominations.Nominations;
using WookiepediaStatusArticleData.Nominations.Projects;

namespace WookiepediaStatusArticleData.Services.Awards.NominatorAwardCalculations;

public class ProjectsNominatorAwardCalculation(WookiepediaDbContext db) : INominatorAwardCalculation
{
    public async Task<IEnumerable<Award>> GenerateAsync(AwardGenerationGroup generationGroup, CancellationToken cancellationToken)
    {
        var projects = db.Set<Project>()
            .Where(p => !p.IsArchived)
            // this sorting makes it appear in the right order when generating awards
            .OrderBy(p => p.Name)
            .ToList();
        
        var awards = new List<Award>();
        
        foreach (var project in projects)
        {
            awards.AddRange(
                await new NominationQueryBuilder("WookieeProject Contributions", "Non-Panelist Overall", project.Name, db)
                    .WithWookieeProject(project)
                    .WithNonPanelistsOnly()
                    .BuildAsync(generationGroup, cancellationToken)
            );
            
            awards.AddRange(
                await new NominationQueryBuilder("WookieeProject Contributions", "Panelist Overall", project.Name, db)
                    .WithWookieeProject(project)
                    .WithPanelistsOnly()
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
                    .WithNonPanelistsOnly()
                    .BuildAsync(generationGroup, cancellationToken)
            );
            
            awards.AddRange(
                await new NominationQueryBuilder("WookieeProject Contributions", "Panelist GAs", project.Name, db)
                    .WithWookieeProject(project)
                    .WithType(NominationType.Good)
                    .WithPanelistsOnly()
                    .BuildAsync(generationGroup, cancellationToken)
            );
            
            awards.AddRange(
                await new NominationQueryBuilder("WookieeProject Contributions", "Non-Panelist FAs", project.Name, db)
                    .WithWookieeProject(project)
                    .WithType(NominationType.Featured)
                    .WithNonPanelistsOnly()
                    .BuildAsync(generationGroup, cancellationToken)
            );
            
            awards.AddRange(
                await new NominationQueryBuilder("WookieeProject Contributions", "Panelist FAs", project.Name, db)
                    .WithWookieeProject(project)
                    .WithType(NominationType.Featured)
                    .WithPanelistsOnly()
                    .BuildAsync(generationGroup, cancellationToken)
            );
            
            awards.AddRange(
                await new NominationQueryBuilder("WookieeProject Contributions", "Non-Panelist Score", project.Name, db)
                    .WithCountMode(CountMode.JocastaBotPoints)
                    .WithWookieeProject(project)
                    .WithNonPanelistsOnly()
                    .BuildAsync(generationGroup, cancellationToken)
            );
            
            awards.AddRange(
                await new NominationQueryBuilder("WookieeProject Contributions", "Panelist Score", project.Name, db)
                    .WithCountMode(CountMode.JocastaBotPoints)
                    .WithWookieeProject(project)
                    .WithPanelistsOnly()
                    .BuildAsync(generationGroup, cancellationToken)
            );
        }

        return awards;
    }
}
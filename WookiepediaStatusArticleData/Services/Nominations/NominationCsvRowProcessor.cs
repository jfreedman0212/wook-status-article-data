using WookiepediaStatusArticleData.Database;
using WookiepediaStatusArticleData.Nominations.Nominations;
using WookiepediaStatusArticleData.Nominations.Nominations.Csv;
using WookiepediaStatusArticleData.Nominations.Nominators;
using WookiepediaStatusArticleData.Nominations.Projects;

namespace WookiepediaStatusArticleData.Services.Nominations;

public class NominationCsvRowProcessor(WookiepediaDbContext db)
{
    public Nomination Convert(
        NominationCsvRow csvRow,
        IDictionary<string, Project> projects,
        IDictionary<string, Nominator> nominators
    )
    {
        DateTime? startedAt = null;
        DateTime? endedAt = null;

        if (csvRow.StartDate != null)
        {
            startedAt = new DateTime(csvRow.StartDate.Value, csvRow.StartTime ?? TimeOnly.MinValue, DateTimeKind.Utc);
        }
        
        if (csvRow.EndDate != null)
        {
            endedAt = new DateTime(csvRow.EndDate.Value, csvRow.EndTime ?? TimeOnly.MaxValue, DateTimeKind.Utc);
        }
        
        var nomination = new Nomination
        {
            ArticleName = csvRow.ArticleName,
            Continuities = csvRow.Continuities,
            Type = csvRow.Type,
            Outcome = csvRow.Outcome,
            StartedAt = startedAt,
            EndedAt = endedAt,
            StartWordCount = csvRow.StartWordCount,
            EndWordCount = csvRow.EndWordCount,
            Nominators = [],
            Projects = []
        };

        foreach (var csvRowNominator in csvRow.Nominators)
        {
            if (!nominators.TryGetValue(csvRowNominator, out var nominator))
            {
                nominator = new Nominator { Name = csvRowNominator };
                
                nominators.Add(csvRowNominator, nominator);
                db.Add(nominator);
            }

            nomination.Nominators!.Add(nominator);
        }

        var now = DateTime.UtcNow;
        
        foreach (var csvRowProject in csvRow.WookieeProjects)
        {
            if (!projects.TryGetValue(csvRowProject, out var project))
            {
                project = new Project
                {
                    Name = csvRowProject,
                    Type = ProjectType.Category,
                    // TODO: do we want to try to infer this somehow?
                    CreatedAt = now,
                    HistoricalValues = 
                    [
                        new HistoricalProject
                        { 
                            ActionType = ProjectActionType.Create,
                            Name = csvRowProject,
                            Type = ProjectType.Category,
                            OccurredAt = now
                        }
                    ]
                };

                projects.Add(csvRowProject, project);
                db.Add(project);
            }

            nomination.Projects!.Add(project);
        }

        return nomination;
    }
}
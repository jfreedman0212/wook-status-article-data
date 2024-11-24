using WookiepediaStatusArticleData.Database;
using WookiepediaStatusArticleData.Models.Projects;
using WookiepediaStatusArticleData.Nominations.Projects;

namespace WookiepediaStatusArticleData.Services.Projects;

public class CreateProjectAction(ProjectValidator validator, WookiepediaDbContext db)
{
    public async Task<Project> ExecuteAsync(ProjectForm form, CancellationToken cancellationToken)
    {
        var issues = validator.ValidateDate(form.CreatedDate, form.CreatedTime)
            .Concat(await validator.ValidateNameAsync(null, form.Name, cancellationToken))
            .ToList();

        if (issues.Count > 0)
        {
            throw new ValidationException(issues);
        }
        
        var createdAt = new DateTime(form.CreatedDate, form.CreatedTime, DateTimeKind.Utc);
        var project = new Project
        {
            Name = form.Name,
            CreatedAt = createdAt,
            Type = form.Type,
            HistoricalValues = 
            [
                new HistoricalProject
                { 
                    ActionType = ProjectActionType.Create,
                    Name = form.Name,
                    Type = form.Type,
                    OccurredAt = createdAt
                }
            ]
        };

        db.Add(project);
        return project;
    }
}
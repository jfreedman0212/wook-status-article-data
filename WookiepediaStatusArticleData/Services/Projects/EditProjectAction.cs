using Microsoft.EntityFrameworkCore;
using WookiepediaStatusArticleData.Database;
using WookiepediaStatusArticleData.Models.Projects;
using WookiepediaStatusArticleData.Nominations.Projects;

namespace WookiepediaStatusArticleData.Services.Projects;

public class EditProjectAction(ProjectValidator validator, WookiepediaDbContext db)
{
    public async Task<Project?> ExecuteAsync(int id, ProjectForm form, CancellationToken cancellationToken)
    {
        form.Id = id;

        var issues = await validator.ValidateNameAsync(id, form.Name, cancellationToken);

        if (issues.Count > 0)
        {
            throw new ValidationException(issues);
        }

        var project = await db.Set<Project>()
            .Include(it => it.HistoricalValues)
            .SingleOrDefaultAsync(it => it.Id == id && !it.IsArchived, cancellationToken);

        if (project == null) return null;

        project.Name = form.Name;
        project.Type = form.Type;
        project.HistoricalValues!.Add(new HistoricalProject
        {
            ActionType = ProjectActionType.Update,
            Name = form.Name,
            Type = form.Type,
            OccurredAt = DateTime.UtcNow
        });

        return project;
    }
}
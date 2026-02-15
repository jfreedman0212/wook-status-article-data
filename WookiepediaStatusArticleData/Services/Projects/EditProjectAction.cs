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

        var project = await db.Set<Project>()
            .Include(it => it.HistoricalValues)
            .SingleOrDefaultAsync(it => it.Id == id && !it.IsArchived, cancellationToken);

        if (project == null) return null;

        var issues = await validator.ValidateNameAsync(id, form.Name, cancellationToken);

        // Only validate code if the project doesn't have one yet (first-time fill)
        if (project.Code is null)
        {
            issues = issues.Concat(await validator.ValidateCodeAsync(id, form.Code, cancellationToken)).ToList();
        }

        if (issues.Count > 0)
        {
            throw new ValidationException(issues);
        }

        project.Name = form.Name;
        project.Type = form.Type;
        project.CreatedAt = form.CreatedDate.ToDateTime(form.CreatedTime, DateTimeKind.Utc);

        // Only assign code if it's currently null (one-time fill)
        if (project.Code is null)
        {
            project.Code = form.Code;
        }

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
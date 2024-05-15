using Microsoft.EntityFrameworkCore;
using WookiepediaStatusArticleData.Database;
using WookiepediaStatusArticleData.Models.Projects;
using WookiepediaStatusArticleData.Nominations.Projects;

namespace WookiepediaStatusArticleData.Services.Projects;

public class ProjectValidator(WookiepediaDbContext db)
{
    public async Task<IList<ValidationIssue>> ValidateNameAsync(
        int? id,
        string name,
        CancellationToken cancellationToken
    )
    {
        var issues = new List<ValidationIssue>();

        var differentProjectWithSameName = await db.Set<Project>()
            .SingleOrDefaultAsync(it => it.Name == name && it.Id != id, cancellationToken);

        if (differentProjectWithSameName != null)
        {
            issues.Add(
                new ValidationIssue(
                    nameof(AddProjectForm.Name),
                    differentProjectWithSameName.IsArchived
                        ? $"{name} already exists as an archived project, choose another name."
                        : $"{name} already exists, choose another name."
                )
            );
        }

        return issues;
    }

    public IList<ValidationIssue> ValidateDate(DateOnly createdDate, TimeOnly createdTime)
    {
        var now = DateTime.UtcNow;
        var nowDate = DateOnly.FromDateTime(now);
        var nowTime = TimeOnly.FromDateTime(now);

        var issues = new List<ValidationIssue>();

        if (nowDate < createdDate)
        {
            issues.Add(
                new ValidationIssue(nameof(AddProjectForm.CreatedDate), "Created Date must be today or in the past.")
            );
        }

        if (nowDate == createdDate && nowTime < createdTime)
        {
            issues.Add(
                new ValidationIssue(nameof(AddProjectForm.CreatedTime), "Created Time must be now or in the past.")
            );
        }

        return issues;
    }
}
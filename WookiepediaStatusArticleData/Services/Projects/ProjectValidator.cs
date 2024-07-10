using Microsoft.EntityFrameworkCore;
using WookiepediaStatusArticleData.Database;
using WookiepediaStatusArticleData.Models.Projects;
using WookiepediaStatusArticleData.Nominations.Projects;

namespace WookiepediaStatusArticleData.Services.Projects;

public class ProjectValidator(WookiepediaDbContext db)
{
    public async Task<IList<ValidationIssue>> ValidateAsync(ProjectForm form, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var nowDate = DateOnly.FromDateTime(now);
        var nowTime = TimeOnly.FromDateTime(now);
        
        var issues = new List<ValidationIssue>();
        
        if (nowDate < form.CreatedDate)
        {
            issues.Add(
                new ValidationIssue(nameof(form.CreatedDate), "Created Date must be today or in the past.")
            );
        }

        if (nowDate == form.CreatedDate && nowTime < form.CreatedTime)
        {
            issues.Add(
                new ValidationIssue(nameof(form.CreatedTime), "Created Time must be now or in the past.")    
            );
        }
        
        var differentProjectWithSameName = await db.Set<Project>()
            .SingleOrDefaultAsync(it => it.Name == form.Name && it.Id != form.Id, cancellationToken);

        if (differentProjectWithSameName != null)
        {
            issues.Add(
                new ValidationIssue(
                    nameof(form.Name),
                    differentProjectWithSameName.IsArchived
                        ? $"{form.Name} already exists as an archived project, choose another name."
                        : $"{form.Name} already exists, choose another name."    
                )
            );
        }

        return issues;
    }
}
using Microsoft.EntityFrameworkCore;
using WookiepediaStatusArticleData.Database;
using WookiepediaStatusArticleData.Models.Nominators;
using WookiepediaStatusArticleData.Nominations.Nominators;

namespace WookiepediaStatusArticleData.Services.Nominators;

public class NominatorValidator(WookiepediaDbContext db)
{
    public async Task<List<ValidationIssue>> ValidateNameAsync(
        int? id,
        string name,
        CancellationToken cancellationToken
    )
    {
        var nominatorWithSameName = await db.Set<Nominator>()
            .AnyAsync(it => it.Name == name && it.Id != id, cancellationToken);

        return nominatorWithSameName
            ? [new ValidationIssue(nameof(NominatorForm.Name), $"{name} is already taken by another user.")]
            : [];
    }

    public IList<ValidationIssue> ValidateEffectiveAsOfDate(
        DateOnly? date,
        TimeOnly? time
    )
    {
        var issues = new List<ValidationIssue>();

        if (date == null)
        {
            issues.Add(new ValidationIssue(
                nameof(NominatorForm.EffectiveAsOfDate),
                "Effective As Of Date is required when there are attribute changes."
            ));
        }

        if (time == null)
        {
            issues.Add(new ValidationIssue(
                nameof(NominatorForm.EffectiveAsOfTime),
                "Effective As Of Time is required when there are attribute changes."
            ));
        }

        return issues;
    }
}
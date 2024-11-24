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
            ?
            [
                new ValidationIssue(
                    nameof(NominatorForm.Name),
                    $"{name} is already used by another nominator."
                ),
            ]
            : [];
    }

    public List<ValidationIssue> ValidateAttributes(NominatorForm form)
    {
        // TODO: implement this!
        return [];
    }
}


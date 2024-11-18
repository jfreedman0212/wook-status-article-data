using Microsoft.EntityFrameworkCore;
using WookiepediaStatusArticleData.Database;
using WookiepediaStatusArticleData.Models.Nominators;
using WookiepediaStatusArticleData.Nominations.Nominators;

namespace WookiepediaStatusArticleData.Services.Nominators;

public class EditNominatorAction(WookiepediaDbContext db, NominatorValidator validator)
{
    public async Task<Nominator?> ExecuteAsync(
        int? id,
        NominatorForm form,
        CancellationToken cancellationToken
    )
    {
        form.Id = id;
        var nominator =
            id != null
                ? await db.Set<Nominator>()
                    .Include(it => it.Attributes!.OrderBy(attr => attr.AttributeName))
                    .SingleOrDefaultAsync(it => it.Id == id, cancellationToken)
                : null;

        if (nominator == null)
        {
            nominator = new Nominator { Name = form.Name, Attributes = [] };
            db.Add(nominator);
        }

        var issues = await validator.ValidateNameAsync(id, form.Name, cancellationToken);

        if (issues.Count > 0)
        {
            throw new ValidationException(issues);
        }

        nominator.Name = form.Name;

        return nominator;
    }
}


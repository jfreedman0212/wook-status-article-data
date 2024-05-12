using Microsoft.EntityFrameworkCore;
using WookiepediaStatusArticleData.Database;
using WookiepediaStatusArticleData.Models.Nominators;
using WookiepediaStatusArticleData.Nominations.Nominators;

namespace WookiepediaStatusArticleData.Services.Nominators;

public class EditNominatorAction(WookiepediaDbContext db)
{
    public async Task<Nominator?> ExecuteAsync(int id, NominatorForm form, CancellationToken cancellationToken)
    {
        var nominator = await db.Set<Nominator>()
            .Include(it => it.Attributes!.OrderBy(attr => attr.AttributeName))
            .SingleOrDefaultAsync(it => it.Id == id, cancellationToken);

        if (nominator == null) return null;

        var nominatorWithSameName = await db.Set<Nominator>().AnyAsync(it => it.Name == form.Name && it.Id != id, cancellationToken);

        if (nominatorWithSameName)
        {
            throw new ValidationException(
                new ValidationIssue(nameof(form.Name), $"{form.Name} is already taken by another user.")
            );
        }

        nominator.Name = form.Name;

        var currentAttributes = nominator.Attributes!.Where(it => it.EffectiveEndAt == null).ToList();

        var now = DateTime.UtcNow;
        
        // add items that don't exist yet
        foreach (var attribute in form.Attributes)
        {
            if (currentAttributes.All(it => it.AttributeName != attribute))
            {
                nominator.Attributes!.Add(new NominatorAttribute
                {
                    AttributeName = attribute,
                    EffectiveAt = now
                });
            }
        }
        
        // remove items that were removed
        foreach (var attribute in currentAttributes)
        {
            if (form.Attributes.Any(it => it == attribute.AttributeName)) continue;

            attribute.EffectiveEndAt = now;
        }

        return nominator;
    }
}
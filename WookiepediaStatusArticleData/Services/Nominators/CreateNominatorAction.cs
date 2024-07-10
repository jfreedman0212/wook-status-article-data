using Microsoft.EntityFrameworkCore;
using WookiepediaStatusArticleData.Database;
using WookiepediaStatusArticleData.Models.Nominators;
using WookiepediaStatusArticleData.Nominations.Nominators;

namespace WookiepediaStatusArticleData.Services.Nominators;

public class CreateNominatorAction(WookiepediaDbContext db)
{
    public async Task<Nominator> ExecuteAsync(NominatorForm form, CancellationToken cancellationToken)
    {
        var nominatorWithSameName = await db.Set<Nominator>().AnyAsync(it => it.Name == form.Name, cancellationToken);

        if (nominatorWithSameName)
        {
            throw new ValidationException(
                new ValidationIssue(nameof(form.Name), $"{form.Name} is already taken by another user.")
            );
        }

        var now = DateTime.UtcNow;
        var newNominator = new Nominator
        {
            Name = form.Name,
            Attributes = form.Attributes.Select(attr => new NominatorAttribute
            {
                AttributeName = attr,
                EffectiveAt = now
            }).ToList()
        };
        
        db.Add(newNominator);

        return newNominator;
    }
}
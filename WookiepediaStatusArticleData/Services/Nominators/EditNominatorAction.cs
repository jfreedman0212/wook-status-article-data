using Microsoft.EntityFrameworkCore;
using WookiepediaStatusArticleData.Database;
using WookiepediaStatusArticleData.Models.Nominators;
using WookiepediaStatusArticleData.Nominations.Nominators;

namespace WookiepediaStatusArticleData.Services.Nominators;

public class EditNominatorAction(
    WookiepediaDbContext db,
    NominatorValidator validator
)
{
    public async Task<Nominator?> ExecuteAsync(int? id, NominatorForm form, CancellationToken cancellationToken)
    {
        var nominator = id != null
            ? await db.Set<Nominator>()
                .Include(it => it.Attributes!.OrderBy(attr => attr.AttributeName))
                .SingleOrDefaultAsync(it => it.Id == id, cancellationToken)
            : null;

        if (nominator == null)
        {
            nominator = new Nominator
            {
                Name = form.Name,
                Attributes = []
            };
            db.Add(nominator);
        }

        var issues = await validator.ValidateNameAsync(id, form.Name, cancellationToken);
        
        if (issues.Count > 0)
        {
            throw new ValidationException(issues);
        }

        nominator.Name = form.Name;
        
        var currentAttributes = nominator.Attributes!
            .Where(it => it.EffectiveEndAt == null)
            .ToDictionary(it => it.AttributeName);

        var hasAttributeChanges = currentAttributes.Count != form.Attributes.Count
            || form.Attributes.Except(currentAttributes.Select(it => it.Key)).Any();
        
        if (hasAttributeChanges)
        {
            issues.AddRange(validator.ValidateEffectiveAsOfDate(form.EffectiveAsOfDate, form.EffectiveAsOfTime));
            
            if (issues.Count > 0)
            {
                throw new ValidationException(issues);
            }

            var now = new DateTime(form.EffectiveAsOfDate!.Value, form.EffectiveAsOfTime!.Value, DateTimeKind.Utc);
            
            // if we're banning them, remove all other attributes
            if (form.Attributes.Contains(NominatorAttributeType.Banned))
            {
                form.Attributes = [NominatorAttributeType.Banned];
            }
        
            // add items that don't exist yet
            foreach (var attribute in form.Attributes)
            {
                if (!currentAttributes.ContainsKey(attribute))
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
                if (form.Attributes.Any(it => it == attribute.Key)) continue;

                if (now <= attribute.Value.EffectiveAt)
                {
                    issues.Add(new ValidationIssue(
                        nameof(form.EffectiveAsOfDate),
                        $"Effective As Of Date and Time must be after the date for {attribute.Key.ToDescription()}, which starts on {attribute.Value.EffectiveAt:s}."    
                    ));
                }
                
                attribute.Value.EffectiveEndAt = now;
            }
        }

        if (issues.Count > 0)
        {
            throw new ValidationException(issues);
        }
        
        return nominator;
    }
}
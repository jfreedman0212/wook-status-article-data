using Microsoft.EntityFrameworkCore;
using WookiepediaStatusArticleData.Database;
using WookiepediaStatusArticleData.Nominations.Nominators;

namespace WookiepediaStatusArticleData.Services.Nominators;

public class BanNominatorAction(WookiepediaDbContext db)
{
    public async Task<Nominator?> BanAsync(int id, CancellationToken cancellationToken)
    {
        var nominator = await db.Set<Nominator>()
            .Include(it => it.Attributes!.OrderBy(attr => attr.AttributeName))
            .SingleOrDefaultAsync(it => it.Id == id, cancellationToken);

        if (nominator == null) return null;
        
        var now = DateTime.UtcNow;
        var alreadyBanned = false;

        foreach (var attribute in nominator.Attributes!)
        {
            if (attribute is { AttributeName: NominatorAttributeType.Banned, EffectiveEndAt: null })
            {
                alreadyBanned = true;
                continue;
            }
            
            attribute.EffectiveEndAt ??= now;
        }

        // if they're already banned, no need to add a new ban record
        if (!alreadyBanned)
        {
            nominator.Attributes.Add(new NominatorAttribute
            {
                AttributeName = NominatorAttributeType.Banned,
                EffectiveAt = now
            });   
        }

        return nominator;
    }

    public async Task<Nominator?> UnbanAsync(int id, CancellationToken cancellationToken)
    {
        var nominator = await db.Set<Nominator>()
            .Include(it => it.Attributes!.OrderBy(attr => attr.AttributeName))
            .SingleOrDefaultAsync(it => it.Id == id, cancellationToken);
        
        if (nominator == null) return null;

        var now = DateTime.UtcNow;

        foreach (var attribute in nominator.Attributes!)
        {
            if (attribute is { AttributeName: NominatorAttributeType.Banned, EffectiveEndAt: null })
            {
                attribute.EffectiveEndAt = now;
            }
        }

        return nominator;
    }
}
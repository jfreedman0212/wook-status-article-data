using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.FeatureManagement.Mvc;
using WookiepediaStatusArticleData.Database;
using WookiepediaStatusArticleData.Features;
using WookiepediaStatusArticleData.Models.Nominators;
using WookiepediaStatusArticleData.Nominations.Nominators;

namespace WookiepediaStatusArticleData.Controllers;

[FeatureGate(FeatureFlags.NominatorManagement)]
[Route("nominators")]
public class NominatorsController(WookiepediaDbContext db) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var nominators = await db.Set<Nominator>()
            .Include(it => it.Attributes!.Where(attr => attr.EffectiveEndAt == null))
            .OrderBy(it => it.Name)
            .ToListAsync(cancellationToken);
        
        return View(new NominatorsViewModel { Nominators = nominators });
    }

    [HttpPost("ban/{id:int}")]
    public async Task<IActionResult> BanNominator([FromRoute] int id, CancellationToken cancellationToken)
    {
        var nominator = await db.Set<Nominator>()
            .Include(it => it.Attributes)
            .SingleOrDefaultAsync(it => it.Id == id, cancellationToken);
        
        if (nominator == null) return NotFound();

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

        await db.SaveChangesAsync(cancellationToken);

        // filter out non-current attributes after saving so they're not deleted from the DB, but also
        // so they're not included in the view
        nominator.Attributes = nominator.Attributes.Where(attr => attr.EffectiveEndAt == null).ToList();
        return PartialView("_Nominator", nominator); 
    }
    
    [HttpPost("un-ban/{id:int}")]
    public async Task<IActionResult> UnbanNominator([FromRoute] int id, CancellationToken cancellationToken)
    {
        var nominator = await db.Set<Nominator>()
            .Include(it => it.Attributes)
            .SingleOrDefaultAsync(it => it.Id == id, cancellationToken);
        
        if (nominator == null) return NotFound();

        var now = DateTime.UtcNow;

        foreach (var attribute in nominator.Attributes!)
        {
            if (attribute is { AttributeName: NominatorAttributeType.Banned, EffectiveEndAt: null })
            {
                attribute.EffectiveEndAt = now;
            }
        }

        await db.SaveChangesAsync(cancellationToken);
        
        // filter out non-current attributes after saving so they're not deleted from the DB, but also
        // so they're not included in the view
        nominator.Attributes = nominator.Attributes.Where(attr => attr.EffectiveEndAt == null).ToList();
        return PartialView("_Nominator", nominator); 
    }
}
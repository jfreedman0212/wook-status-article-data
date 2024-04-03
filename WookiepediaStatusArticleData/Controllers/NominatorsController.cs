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
            .Include(it => it.Attributes!.Where(attr => attr.EffectiveEndAt == null).OrderBy(attr => attr.AttributeName))
            .OrderBy(it => it.Name)
            .ToListAsync(cancellationToken);
        
        return View(new NominatorsViewModel { Nominators = nominators });
    }

    [HttpGet("{id:int}/view")]
    public async Task<IActionResult> IndividualNominator(
        [FromRoute] int id,
        CancellationToken cancellationToken
    )
    {
        var nominator = await db.Set<Nominator>()
            .Include(it => it.Attributes!.Where(attr => attr.EffectiveEndAt == null).OrderBy(attr => attr.AttributeName))
            .SingleOrDefaultAsync(it => it.Id == id, cancellationToken);

        if (nominator == null) return NotFound();
        
        return PartialView("_Nominator", nominator);
    }

    [HttpGet("{id:int}/edit")]
    public async Task<IActionResult> EditForm(
        [FromRoute] int id,
        CancellationToken cancellationToken
    )
    {
        var nominator = await db.Set<Nominator>()
            .Include(it => it.Attributes!.Where(attr => attr.EffectiveEndAt == null).OrderBy(attr => attr.AttributeName))
            .SingleOrDefaultAsync(it => it.Id == id, cancellationToken);

        if (nominator == null) return NotFound();

        return PartialView("_Nominator.Edit", new NominatorEditViewModel
        {
            Id = nominator.Id,
            Name = nominator.Name,
            Attributes = nominator.Attributes!.Select(it => it.AttributeName).ToList(),
            AllowedAttributes = Enum.GetValues<NominatorAttributeType>()
                .Where(attr => attr != NominatorAttributeType.Banned)
                .ToList()
        });
    }
        
    [HttpPost("{id:int}/ban")]
    public async Task<IActionResult> BanNominator([FromRoute] int id, CancellationToken cancellationToken)
    {
        var nominator = await db.Set<Nominator>()
            .Include(it => it.Attributes!.OrderBy(attr => attr.AttributeName))
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
    
    [HttpPost("{id:int}/un-ban")]
    public async Task<IActionResult> UnbanNominator([FromRoute] int id, CancellationToken cancellationToken)
    {
        var nominator = await db.Set<Nominator>()
            .Include(it => it.Attributes!.OrderBy(attr => attr.AttributeName))
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

    [HttpPost("{id:int}/edit")]
    public async Task<IActionResult> Edit(
        [FromRoute] int id,
        [FromForm] NominatorEditViewModel form,
        CancellationToken cancellationToken
    )
    {
        var nominator = await db.Set<Nominator>()
            .Include(it => it.Attributes!.OrderBy(attr => attr.AttributeName))
            .SingleOrDefaultAsync(it => it.Id == id, cancellationToken);

        if (nominator == null) return NotFound();

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

        await db.SaveChangesAsync(cancellationToken);

        // filter out non-current attributes after saving so they're not deleted from the DB, but also
        // so they're not included in the view
        nominator.Attributes = nominator.Attributes!.Where(attr => attr.EffectiveEndAt == null).ToList();
        return PartialView("_Nominator", nominator);
    }
}
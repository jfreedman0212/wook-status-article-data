using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.FeatureManagement.Mvc;
using WookiepediaStatusArticleData.Database;
using WookiepediaStatusArticleData.Features;
using WookiepediaStatusArticleData.Models.Nominators;
using WookiepediaStatusArticleData.Nominations;

namespace WookiepediaStatusArticleData.Controllers;

[FeatureGate(FeatureFlags.NominatorManagement)]
[Route("nominators")]
public class NominatorsController(WookiepediaDbContext db) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var nominators = await db.Set<Nominator>()
            .Include(it => it.Attributes)
            .OrderBy(it => it.Name)
            .ToListAsync(cancellationToken);
        
        return View(new NominatorsViewModel { Nominators = nominators });
    }

    [HttpGet("edit/{id:int}")]
    public async Task<IActionResult> EditForm([FromRoute] int id, CancellationToken cancellationToken)
    {
        var nominator = await db.Set<Nominator>()
            .Include(it => it.Attributes)
            .SingleOrDefaultAsync(it => it.Id == id, cancellationToken);

        if (nominator == null) return NotFound();
        
        return View(NominatorForm.From(nominator));
    }
}
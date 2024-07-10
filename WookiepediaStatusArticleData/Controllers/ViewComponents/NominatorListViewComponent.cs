using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WookiepediaStatusArticleData.Database;
using WookiepediaStatusArticleData.Models.Nominators;
using WookiepediaStatusArticleData.Nominations.Nominators;

namespace WookiepediaStatusArticleData.Controllers.ViewComponents;

public class NominatorListViewComponent(WookiepediaDbContext db) : ViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync()
    {
        var nominators = await db.Set<Nominator>()
            .Include(it => it.Attributes!.Where(attr => attr.EffectiveEndAt == null).OrderBy(attr => attr.AttributeName))
            .OrderBy(it => it.Name)
            .ToListAsync();
        
        return View(new NominatorsViewModel { Nominators = nominators });
    }
}
using Microsoft.AspNetCore.Mvc;
using WookiepediaStatusArticleData.Models.Nominators;
using WookiepediaStatusArticleData.Nominations.Nominators;

namespace WookiepediaStatusArticleData.Controllers.ViewComponents;

public class NominatorViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(Nominator nominator)
    {
        return View(new NominatorViewModel
        {
            Id = nominator.Id,
            Name = nominator.Name,
            // we only care about current attributes
            Attributes = nominator.Attributes!
                .Where(attr => attr.EffectiveEndAt == null)
                .ToList()
        });
    }
}
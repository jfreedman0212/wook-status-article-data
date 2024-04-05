using Microsoft.AspNetCore.Mvc;
using WookiepediaStatusArticleData.Nominations.Nominators;

namespace WookiepediaStatusArticleData.Controllers.ViewComponents;

public class NominatorViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(Nominator nominator)
    {
        return View(nominator);
    }
}
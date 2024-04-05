using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WookiepediaStatusArticleData.Database;
using WookiepediaStatusArticleData.Models.Nominators;
using WookiepediaStatusArticleData.Nominations.Nominators;

namespace WookiepediaStatusArticleData.Controllers;

public class NominatorFormViewComponent : ViewComponent
{
    private static readonly IList<NominatorAttributeType> AllowedAttributes = Enum.GetValues<NominatorAttributeType>()
        .Where(attr => attr != NominatorAttributeType.Banned)
        .ToList();
    
    public IViewComponentResult Invoke(NominatorForm? form)
    {
        var nominatorForm = form ?? new NominatorForm
        {
            Id = 0,
            Name = "",
            Attributes = []
        };
        
        return View(new NominatorEditViewModel
        {
            Id = nominatorForm.Id,
            Name = nominatorForm.Name,
            Attributes = nominatorForm.Attributes,
            AllowedAttributes = AllowedAttributes
        });
    }
}
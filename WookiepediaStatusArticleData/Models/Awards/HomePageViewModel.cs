using Microsoft.AspNetCore.Mvc.Rendering;

namespace WookiepediaStatusArticleData.Models.Awards;

public class HomePageViewModel
{
    public required AwardGenerationGroupDetailViewModel? Selected { get; init; }
    public required IList<SelectListItem> Groups { get; init; }
}

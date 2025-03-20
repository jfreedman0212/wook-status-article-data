using Microsoft.AspNetCore.Mvc.Rendering;
using WookiepediaStatusArticleData.Nominations.Nominators;
using WookiepediaStatusArticleData.Nominations.Projects;

namespace WookiepediaStatusArticleData.Models.Awards;

public class HomePageViewModel
{
    public required AwardGenerationGroupDetailViewModel? Selected { get; init; }
    public required IList<SelectListItem> Groups { get; init; }
    public IList<Nominator> NominatorsWhoParticipatedButDidntPlace { get; init; } = [];
    public IList<Project> AddedProjects { get; init; } = [];
    public int TotalFirstPlaceAwards { get; init; } = 0;
}

public class ProjectCountProjection
{
    public required Project Project { get; init; }
    public required int Count { get; init; }
}

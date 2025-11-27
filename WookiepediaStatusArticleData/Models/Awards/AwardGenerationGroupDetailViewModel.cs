using WookiepediaStatusArticleData.Nominations.Nominations;
using WookiepediaStatusArticleData.Nominations.Nominators;
using WookiepediaStatusArticleData.Nominations.Projects;

namespace WookiepediaStatusArticleData.Models.Awards;

public class AwardGenerationGroupDetailViewModel
{
    public required int Id { get; init; }
    public required string Name { get; init; }
    public required DateTime StartedAt { get; init; }
    public required DateTime EndedAt { get; init; }
    public required IList<AwardHeadingViewModel> AwardHeadings { get; init; }
    public IList<Nominator> NominatorsWhoParticipatedButDidntPlace { get; init; } = [];
    public IList<Project> AddedProjects { get; init; } = [];
    public int TotalFirstPlaceAwards { get; init; } = 0;
    public IList<Nomination> NominationsWithMostWookieeProjects { get; init; } = [];
}
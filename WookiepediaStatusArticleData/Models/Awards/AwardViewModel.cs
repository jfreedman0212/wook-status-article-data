using WookiepediaStatusArticleData.Nominations.Nominators;
using WookiepediaStatusArticleData.Nominations.Projects;

namespace WookiepediaStatusArticleData.Models.Awards;

public class AwardHeadingViewModel
{
    public required string Heading { get; init; }
    public string HeadingSlug => Heading.Replace(" ", "-");
    public required List<SubheadingAwardViewModel> Subheadings { get; init; }
}

public enum TableMode
{
    Default,
    WookieeProject,
    MVP,
    MostDaysWithArticles,
    LongestStatusArticle,
}

public class SubheadingAwardViewModel
{
    public required TableMode Mode { get; init; }
    public required string Subheading { get; init; }
    public string SubheadingSlug => Subheading.Replace(" ", "-");
    public required IList<AwardViewModel> Awards { get; init; }
}

public class AwardViewModel
{
    public required int Order { get; init; }
    public required string Heading { get; init; }
    public required string Subheading { get; init; }
    public required string Type { get; init; }
    public required IList<WinnerViewModel> Winners { get; init; }
}

public class WinnerViewModel
{
    public required IList<WinnerNameViewModel> Names { get; init; }
    public required int Count { get; init; }
}

public record WinnerNameViewModel 
{
    public record NominatorView(Nominator Nominator) : WinnerNameViewModel();
    public record WookieeProject(string ProjectName) : WinnerNameViewModel();
    public record Date(DateOnly DateOfNomination) : WinnerNameViewModel();
}
namespace WookiepediaStatusArticleData.Models.Awards;

public class AwardHeadingViewModel
{
    public required string Heading { get; init; }
    public required List<SubheadingAwardViewModel> Subheadings { get; init; }
}

public class SubheadingAwardViewModel
{
    public required string Subheading { get; init; }
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
    public required IList<string> Names { get; init; }
    public required int Count { get; init; }
}

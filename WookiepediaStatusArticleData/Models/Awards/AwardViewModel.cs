using System.Collections;

namespace WookiepediaStatusArticleData.Models.Awards;

public class AwardViewModel
{
    public required int Type { get; init; }
    public required IList<WinnerViewModel> Winners { get; init; }
}

public class WinnerViewModel
{
    public required IList<string> Names { get; init; }
    public required int Count { get; init; }
}

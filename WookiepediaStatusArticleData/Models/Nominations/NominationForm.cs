using WookiepediaStatusArticleData.Nominations.Nominations;

namespace WookiepediaStatusArticleData.Models.Nominations;

public class NominationForm
{
    public int Id { get; init; }
    public required string ArticleName { get; init; }
    public required IList<Continuity> Continuities { get; init; }
    public required NominationType Type { get; init; }
    public required Outcome Outcome { get; init; }
    public required DateOnly StartedAtDate { get; init; }
    public required TimeOnly StartedAtTime { get; init; }
    public DateOnly? EndedAtDate { get; init; }
    public TimeOnly? EndedAtTime { get; init; }
    public required int StartWordCount { get; init; }
    public required int EndWordCount { get; init; }
    public required IList<int> NominatorIds { get; init; }
    public required IList<int> ProjectIds { get; init; }
}

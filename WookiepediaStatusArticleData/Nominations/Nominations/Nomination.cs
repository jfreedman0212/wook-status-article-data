using WookiepediaStatusArticleData.Nominations.Nominators;
using WookiepediaStatusArticleData.Nominations.Projects;

namespace WookiepediaStatusArticleData.Nominations.Nominations;

public class Nomination
{
    public int Id { get; set; }
    public required string ArticleName { get; set; }
    public required IList<Continuity> Continuities { get; set; }
    public required NominationType Type { get; set; }
    public required Outcome Outcome { get; set; }
    public required DateTime StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public int? StartWordCount { get; set; }
    public int? EndWordCount { get; set; }

    public IList<Nominator>? Nominators { get; set; }
    public IList<Project>? Projects { get; set; }
}
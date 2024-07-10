using WookiepediaStatusArticleData.Nominations.Nominations;

namespace WookiepediaStatusArticleData.Models.Nominations;

public class NominationViewModel
{
    public required int Id { get; set; }
    public required string ArticleName { get; set; }
    public required IList<Continuity> Continuities { get; set; }
    public required NominationType Type { get; set; }
    public required Outcome Outcome { get; set; }
    public required DateTime StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public int? StartWordCount { get; set; }
    public int? EndWordCount { get; set; }
    
    public required IList<NominationNominatorViewModel> Nominators { get; set; }
    public required IList<NominationProjectViewModel> Projects { get; set; }
}
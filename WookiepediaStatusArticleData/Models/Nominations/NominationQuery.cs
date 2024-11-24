using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
using WookiepediaStatusArticleData.Nominations.Nominations;

namespace WookiepediaStatusArticleData.Models.Nominations;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public class NominationQuery
{
    public DateTime? LastStartedAt { get; set; }
    public int LastId { get; init; }
    public string Order { get; init; } = "desc";
    public Continuity? Continuity { get; init; }
    public NominationType? Type { get; init; }
    public Outcome? Outcome { get; init; }
    public DateOnly? StartedAt { get; init; }
    public DateOnly? EndedAt { get; init; }
    public int PageSize { get; init; } = 100;
    public int VisibleRows { get; init; }
    public int? ProjectId { get; set; }
    public int? NominatorId { get; init; }
}
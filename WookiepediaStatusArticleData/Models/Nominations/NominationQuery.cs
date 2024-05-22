using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
using WookiepediaStatusArticleData.Nominations.Nominations;

namespace WookiepediaStatusArticleData.Models.Nominations;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public class NominationQuery
{
    [FromQuery(Name = "lastStartedAt")]
    public DateTime? LastStartedAt { get; set; }
    
    [FromQuery(Name = "lastId")]
    public int LastId { get; init; }

    [FromQuery(Name = "order")]
    public string Order { get; init; } = "desc";

    [FromQuery(Name = "continuity")]
    public Continuity? Continuity { get; init; }
    
    [FromQuery(Name = "type")]
    public NominationType? Type { get; init; }
    
    [FromQuery(Name = "outcome")]
    public Outcome? Outcome { get; init; }
    
    [FromQuery(Name = "startedAt")]
    public DateOnly? StartedAt { get; init; }
    
    [FromQuery(Name = "endedAt")]
    public DateOnly? EndedAt { get; init; }
    
    [FromQuery(Name = "pageSize")]
    public int PageSize { get; init; } = 100;
    
    [FromQuery(Name = "projectId")]
    public int? ProjectId { get; set; }
    
    [FromQuery(Name = "nominatorId")]
    public int? NominatorId { get; init; }
}
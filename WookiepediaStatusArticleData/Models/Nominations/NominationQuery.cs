using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;

namespace WookiepediaStatusArticleData.Models.Nominations;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public class NominationQuery
{
    [FromQuery(Name = "lastStartedAt")]
    public DateTime LastStartedAt { get; init; } = new(DateOnly.MaxValue, TimeOnly.MaxValue, DateTimeKind.Utc);
    
    [FromQuery(Name = "lastId")]
    public int LastId { get; init; }

    [FromQuery(Name = "q")]
    public string? Search { get; init; }

    [FromQuery(Name = "pageSize")]
    public int PageSize { get; init; } = 500;
}
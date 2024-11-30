namespace WookiepediaStatusArticleData.Services.Nominators.AttributeTimeline;

public class TimelineDirective
{
    public required string Identifier { get; init; }

    public required IList<IList<TimelineDirectiveValue>> AttributeLines { get; init; }
}
namespace WookiepediaStatusArticleData.Services.Nominators.AttributeTimeline;

public class TimelineDirective
{
    public required string Identifier { get; init; }

    public required IList<IDictionary<string, string>> AttributeLines { get; init; }
}
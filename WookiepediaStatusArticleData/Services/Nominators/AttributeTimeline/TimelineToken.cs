namespace WookiepediaStatusArticleData.Services.Nominators.AttributeTimeline;

public class TimelineToken
{
    public required TimelineTokenType Type { get; init; }
    public required string Lexeme { get; init; }
}
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using JetBrains.Annotations;

namespace WookiepediaStatusArticleData.Nominations.Nominations.Csv;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public class NominationCsvRow
{
    public required IList<string> Nominators { get; init; }
    public required string ArticleName { get; init; }
    public required IList<Continuity> Continuities { get; init; }
    public required NominationType Type { get; init; }
    public required Outcome Outcome { get; init; }
    public TimeOnly? StartTime { get; init; }
    public required DateOnly StartDate { get; init; }
    public TimeOnly? EndTime { get; init; }
    public DateOnly? EndDate { get; init; }
    public int? StartWordCount { get; init; }
    public int? EndWordCount { get; init; }
    public required IList<string> WookieeProjects { get; init; }
}

[UsedImplicitly]
public sealed class NominationCsvRowClassMap : ClassMap<NominationCsvRow>
{
    public NominationCsvRowClassMap()
    {
        Map(m => m.Nominators)
            .Name("Nominator")
            .Validate(args => !string.IsNullOrWhiteSpace(args.Field), _ => "Nominator must not be empty")
            .Convert(
                args => args.Row.GetField("Nominator")
                    ?.Split(";")
                    .Select(str => str.Trim())
                    .ToList()
            );

        Map(m => m.ArticleName)
            .Name("Article")
            .Validate(args => !string.IsNullOrWhiteSpace(args.Field), _ => "Article must not be empty");
        
        Map(m => m.Continuities)
            .Name("Continuity")
            .Validate(args =>
                {
                    var continuity = args.Row.GetField("Continuity");
                    return string.IsNullOrWhiteSpace(continuity) || !continuity
                        .Split(",")
                        .Select(str => str.Trim())
                        .Select(str => ContinuityExtensions.TryParseFromCode(str, out var code) ? code : null)
                        .Contains(null);
                },
                _ => "Continuity must be one of the following values: Legends, OOU, Canon, Non-Canon, Non-Legends"
            )
            .TypeConverter<ContinuityTypeConverter>();
        
        Map(m => m.Type)
            .Name("Nomination Type")
            .Validate(args => NominationTypes.TryParseFromCode(args.Row.GetField("Nomination Type"), out _), _ => "Nomination Type must be one of the following values: FAN, CAN, GAN")
            .Convert(args =>
            {
                if (NominationTypes.TryParseFromCode(args.Row.GetField("Nomination Type"), out var result)) return result!.Value;

                throw new Exception("This should not happen!");
            });
        
        Map(m => m.Outcome)
            .Name("Outcome")
            .Validate(args => Outcomes.TryParseFromCode(args.Row.GetField("Outcome"), out _), _ => "Outcome must be one of the following values: successful, unsuccessful, withdrawn, other")
            .Convert(args =>
            {
                if (Outcomes.TryParseFromCode(args.Row.GetField("Outcome"), out var result)) return result!.Value;

                throw new Exception("This should not happen!");
            });
        
        Map(m => m.StartTime).Name("Start Time");
        
        Map(m => m.StartDate).Name("Start Date");
        
        Map(m => m.EndTime).Name("End Time");
        
        Map(m => m.EndDate).Name("End Date");
        
        Map(m => m.StartWordCount).Name("Start Word Count");
        
        Map(m => m.EndWordCount).Name("End Word Count");
        
        Map(m => m.WookieeProjects).Name("Wookiee Projects")
            .Convert(
                args => args.Row.GetField("Wookiee Projects")
                    ?.Split(";")
                    .Select(str => str.Trim())
                    .Where(str => !string.IsNullOrWhiteSpace(str))
                    .ToList()
            );
    }
}

[UsedImplicitly]
internal class ContinuityTypeConverter : DefaultTypeConverter
{
    public override object ConvertFromString(string? text, IReaderRow row, MemberMapData memberMapData)
    {
        if (string.IsNullOrWhiteSpace(text)) return new List<Continuity>();
        
        return text
            .Split(",")
            .Select(str => str.Trim())
            .Select(str =>
                ContinuityExtensions.TryParseFromCode(str, out var code)
                    ? code!.Value
                    : throw new Exception("This should not happen!"))
            .ToList();
    }

    public override string ConvertToString(object? value, IWriterRow row, MemberMapData memberMapData)
    {
        throw new NotImplementedException("This function is not ever needed!");
    }
}

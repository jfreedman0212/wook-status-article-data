using System.Text.RegularExpressions;
using WookiepediaStatusArticleData.Models.Nominators;
using WookiepediaStatusArticleData.Nominations.Nominators;

namespace WookiepediaStatusArticleData.Services.Nominators.AttributeTimeline;

public class NominatorAttributeExtractor : IDisposable
{
    private readonly string _dateFormat;
    private readonly PeekEnumerator<IDictionary<string, string>> _plotDataAttributeLines;

    public NominatorAttributeExtractor(IEnumerable<TimelineDirective> directives)
    {
        // I only care about these 2, so skip any others
        using var enumerator = directives
            .Where(d => d.Identifier is "DateFormat" or "PlotData")
            .GetEnumerator();

        enumerator.MoveNext();
        var dateFormatDirective = enumerator.Current;
        enumerator.MoveNext();
        var plotDataDirective = enumerator.Current;

        // we assume there's exactly one line that contains one value. the identifier contains
        // the date format (THIS IS THE ONLY ATTRIBUTE THAT IS A BARE VALUE)
        _dateFormat = dateFormatDirective
            .AttributeLines
            .SingleOrDefault()
            ?.SingleOrDefault()
            .Key ?? "dd/mm/yyyy";

        if (_dateFormat == "dd/mm/yyyy")
        {
            _dateFormat = "dd/MM/yyyy";
        }

        _plotDataAttributeLines = new PeekEnumerator<IDictionary<string, string>>(
            plotDataDirective
                .AttributeLines
                .Where(line => !line.ContainsKey("barset") || line["barset"] != "break")
                .GetEnumerator()
        );
    }

    public IEnumerable<NominatorForm> Extract()
    {
        while (_plotDataAttributeLines.MoveNext())
        {
            var line = _plotDataAttributeLines.Current;

            if (line.ContainsKey("barset"))
            {
                yield return ExtractNominatorFromBarset();
            }
            else if (line.ContainsKey("bar"))
            {
                yield return ExtractNominatorFromBar(line);
            }

            // we only care about lines that start with bar or barset, so skip other lines
        }
    }

    private NominatorForm ExtractNominatorFromBarset()
    {
        string? nominatorName = null;
        var attributes = new List<NominatorAttributeViewModel>();

        while (
            !(_plotDataAttributeLines.Peek?.ContainsKey("barset") ?? false)
            && !(_plotDataAttributeLines.Peek?.ContainsKey("bar") ?? false)
        )
        {
            _plotDataAttributeLines.MoveNext();
            var line = _plotDataAttributeLines.Current;
            nominatorName ??= ParseNominatorName(line["text"]);

            var (startedAt, endedAt) = ParseDateRange(line);

            attributes.Add(new NominatorAttributeViewModel
            {
                AttributeName = ParseAttributeType(line["color"]),
                EffectiveAt = startedAt,
                EffectiveUntil = endedAt
            });
        }

        return new NominatorForm
        {
            Name = nominatorName
                   ?? throw new InvalidOperationException("Nominator name must appear in text attribute on first line of barset"),
            Attributes = MergeDateRanges(
                attributes
                    .OrderBy(attr => attr.AttributeName)
                    .ThenBy(attr => attr.EffectiveAt)
                    .ToList()
            )
        };
    }

    private NominatorForm ExtractNominatorFromBar(IDictionary<string, string> startingLine)
    {
        var (startedAt, endedAt) = ParseDateRange(startingLine);

        return new NominatorForm
        {
            Name = ParseNominatorName(startingLine["text"]),
            Attributes =
            [
                new NominatorAttributeViewModel
                {
                    AttributeName = ParseAttributeType(startingLine["color"]),
                    EffectiveAt = startedAt,
                    EffectiveUntil = endedAt
                }
            ]
        };
    }

    private static NominatorAttributeType ParseAttributeType(string role)
    {
        return role switch
        {
            "inq" => NominatorAttributeType.Inquisitor,
            "ac" => NominatorAttributeType.AcMember,
            "ec" => NominatorAttributeType.EduCorp,
            _ => throw new Exception($"Unexpected attribute name {role}")
        };
    }

    private (DateOnly, DateOnly?) ParseDateRange(IDictionary<string, string> line)
    {
        var startedAt = DateOnly.ParseExact(line["from"], _dateFormat);
        DateOnly? endedAt = line["till"] == "end" ? null : DateOnly.ParseExact(line["till"], _dateFormat);

        return (startedAt, endedAt);
    }

    private static string ParseNominatorName(string textValue)
    {
        var regex = new Regex(@"\[\[User:.+\|.+\]\]");

        if (!regex.IsMatch(textValue))
        {
            throw new Exception($"Unexpected name {textValue}");
        }

        return textValue
            .Replace("[", "")
            .Replace("]", "")
            .Split("|")[1];
    }

    private static List<NominatorAttributeViewModel> MergeDateRanges(List<NominatorAttributeViewModel> ranges)
    {
        if (ranges is not { Count: > 1 }) return ranges;

        var result = new List<NominatorAttributeViewModel>();
        var current = ranges[0];

        for (var i = 1; i < ranges.Count; i++)
        {
            var next = ranges[i];

            // Check if ranges can be merged
            if (
                current.AttributeName == next.AttributeName
                && current.EffectiveUntil.HasValue
                && current.EffectiveUntil.Value >= next.EffectiveAt
            )
            {
                // Merge the ranges by keeping the start of current and end of next
                current = new NominatorAttributeViewModel
                {
                    AttributeName = current.AttributeName,
                    EffectiveAt = current.EffectiveAt,
                    EffectiveUntil = next.EffectiveUntil.HasValue
                        ? (current.EffectiveUntil.Value > next.EffectiveUntil.Value
                            ? current.EffectiveUntil
                            : next.EffectiveUntil)
                        : null
                };
            }
            else
            {
                // Ranges can't be merged, add current to result and move to next
                result.Add(current);
                current = next;
            }
        }

        // Add the last range
        result.Add(current);

        return result;
    }


    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _plotDataAttributeLines.Dispose();
    }
}
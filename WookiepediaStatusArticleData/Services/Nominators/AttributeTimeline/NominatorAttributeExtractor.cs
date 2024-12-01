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
        IList<NominatorAttributeViewModel> attributes = new List<NominatorAttributeViewModel>();
        
        while (
            !(_plotDataAttributeLines.Peek?.ContainsKey("barset") ?? false) 
            && !(_plotDataAttributeLines.Peek?.ContainsKey("bar") ?? false)
        )
        {
            _plotDataAttributeLines.MoveNext();
            var line  = _plotDataAttributeLines.Current;
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
            Attributes = attributes
                .OrderBy(it => it.EffectiveAt)
                .ToList()
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

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _plotDataAttributeLines.Dispose();
    }
}
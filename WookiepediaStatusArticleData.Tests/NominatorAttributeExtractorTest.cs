using WookiepediaStatusArticleData.Models.Nominators;
using WookiepediaStatusArticleData.Nominations.Nominators;
using WookiepediaStatusArticleData.Services.Nominators.AttributeTimeline;

namespace WookiepediaStatusArticleData.Tests;

public class NominatorAttributeExtractorTest
{
    [Fact]
    public void Constructor_WithValidDirectives_InitializesCorrectly()
    {
        var directives = new[]
        {
            new TimelineDirective
            {
                Identifier = "DateFormat",
                AttributeLines =
                [
                    new Dictionary<string, string> { { "dd/MM/yyyy", "" } }
                ]
            },
            new TimelineDirective
            {
                Identifier = "PlotData",
                AttributeLines =
                [
                    new Dictionary<string, string>
                    {
                        { "bar", "[[User:TestUser|TestUser]]" },
                        { "from", "01/01/2020" },
                        { "till", "01/02/2020" },
                        { "color", "inq" },
                        { "text", "[[User:TestUser|TestUser]]" }
                    }
                ]
            }
        };

        using var extractor = new NominatorAttributeExtractor(directives);
        var result = extractor.Extract().ToList();

        Assert.NotNull(result);
        Assert.Single(result);
    }

    [Fact]
    public void Constructor_WithDefaultDateFormat_UsesStandardFormat()
    {
        var directives = new[]
        {
            new TimelineDirective
            {
                Identifier = "DateFormat",
                AttributeLines = []
            },
            new TimelineDirective
            {
                Identifier = "PlotData",
                AttributeLines =
                [
                    new Dictionary<string, string>
                    {
                        { "bar", "[[User:TestUser|TestUser]]" },
                        { "from", "01/01/2020" },
                        { "till", "01/02/2020" },
                        { "color", "inq" },
                        { "text", "[[User:TestUser|TestUser]]" }
                    }
                ]
            }
        };

        using var extractor = new NominatorAttributeExtractor(directives);
        var result = extractor.Extract().ToList();

        Assert.Single(result);
    }

    [Fact]
    public void Constructor_WithCustomDateFormat_UsesCustomFormat()
    {
        var directives = new[]
        {
            new TimelineDirective
            {
                Identifier = "DateFormat",
                AttributeLines =
                [
                    new Dictionary<string, string> { { "yyyy-MM-dd", "" } }
                ]
            },
            new TimelineDirective
            {
                Identifier = "PlotData",
                AttributeLines =
                [
                    new Dictionary<string, string>
                    {
                        { "bar", "[[User:TestUser|TestUser]]" },
                        { "from", "2020-01-01" },
                        { "till", "2020-02-01" },
                        { "color", "inq" },
                        { "text", "[[User:TestUser|TestUser]]" }
                    }
                ]
            }
        };

        using var extractor = new NominatorAttributeExtractor(directives);
        var result = extractor.Extract().ToList();

        Assert.Single(result);
        Assert.Equal(new DateOnly(2020, 1, 1), result[0].Attributes[0].EffectiveAt);
        Assert.Equal(new DateOnly(2020, 2, 1), result[0].Attributes[0].EffectiveUntil);
    }

    [Fact]
    public void Extract_WithBarEntry_ReturnsNominatorForm()
    {
        var directives = CreateDirectivesWithBar(
            "[[User:TestUser|TestUser]]",
            "01/01/2020",
            "01/02/2020",
            "inq"
        );

        using var extractor = new NominatorAttributeExtractor(directives);
        var result = extractor.Extract().ToList();

        Assert.Single(result);
        var nominator = result[0];
        Assert.Equal("TestUser", nominator.Name);
        Assert.False(nominator.IsRedacted);
        Assert.Single(nominator.Attributes);
        Assert.Equal(NominatorAttributeType.Inquisitor, nominator.Attributes[0].AttributeName);
        Assert.Equal(new DateOnly(2020, 1, 1), nominator.Attributes[0].EffectiveAt);
        Assert.Equal(new DateOnly(2020, 2, 1), nominator.Attributes[0].EffectiveUntil);
    }

    [Fact]
    public void Extract_WithBarEntryAndEndDate_ReturnsNominatorWithEndDate()
    {
        var directives = CreateDirectivesWithBar(
            "[[User:TestUser|TestUser]]",
            "01/01/2020",
            "end",
            "ac"
        );

        using var extractor = new NominatorAttributeExtractor(directives);
        var result = extractor.Extract().ToList();

        Assert.Single(result);
        var nominator = result[0];
        Assert.Equal("TestUser", nominator.Name);
        Assert.Equal(NominatorAttributeType.AcMember, nominator.Attributes[0].AttributeName);
        Assert.Equal(new DateOnly(2020, 1, 1), nominator.Attributes[0].EffectiveAt);
        Assert.Null(nominator.Attributes[0].EffectiveUntil);
    }

    [Fact]
    public void Extract_WithMultipleBarEntries_ReturnsMultipleNominators()
    {
        var directives = new[]
        {
            new TimelineDirective
            {
                Identifier = "DateFormat",
                AttributeLines =
                [
                    new Dictionary<string, string> { { "dd/MM/yyyy", "" } }
                ]
            },
            new TimelineDirective
            {
                Identifier = "PlotData",
                AttributeLines =
                [
                    new Dictionary<string, string>
                    {
                        { "bar", "[[User:FirstUser|FirstUser]]" },
                        { "from", "01/01/2020" },
                        { "till", "01/06/2020" },
                        { "color", "inq" },
                        { "text", "[[User:FirstUser|FirstUser]]" }
                    },
                    new Dictionary<string, string>
                    {
                        { "bar", "[[User:SecondUser|SecondUser]]" },
                        { "from", "01/07/2020" },
                        { "till", "end" },
                        { "color", "ac" },
                        { "text", "[[User:SecondUser|SecondUser]]" }
                    }
                ]
            }
        };

        using var extractor = new NominatorAttributeExtractor(directives);
        var result = extractor.Extract().ToList();

        Assert.Equal(2, result.Count);

        var firstNominator = result[0];
        Assert.Equal("FirstUser", firstNominator.Name);
        Assert.Equal(NominatorAttributeType.Inquisitor, firstNominator.Attributes[0].AttributeName);
        Assert.Equal(new DateOnly(2020, 1, 1), firstNominator.Attributes[0].EffectiveAt);
        Assert.Equal(new DateOnly(2020, 6, 1), firstNominator.Attributes[0].EffectiveUntil);

        var secondNominator = result[1];
        Assert.Equal("SecondUser", secondNominator.Name);
        Assert.Equal(NominatorAttributeType.AcMember, secondNominator.Attributes[0].AttributeName);
        Assert.Equal(new DateOnly(2020, 7, 1), secondNominator.Attributes[0].EffectiveAt);
        Assert.Null(secondNominator.Attributes[0].EffectiveUntil);
    }

    [Fact]
    public void Extract_FiltersOutBreakBarsets()
    {
        var directives = new[]
        {
            new TimelineDirective
            {
                Identifier = "DateFormat",
                AttributeLines =
                [
                    new Dictionary<string, string> { { "dd/MM/yyyy", "" } }
                ]
            },
            new TimelineDirective
            {
                Identifier = "PlotData",
                AttributeLines =
                [
                    new Dictionary<string, string> { { "barset", "break" } },
                    new Dictionary<string, string>
                    {
                        { "bar", "[[User:TestUser|TestUser]]" },
                        { "from", "01/01/2020" },
                        { "till", "01/02/2020" },
                        { "color", "inq" },
                        { "text", "[[User:TestUser|TestUser]]" }
                    }
                ]
            }
        };

        using var extractor = new NominatorAttributeExtractor(directives);
        var result = extractor.Extract().ToList();

        Assert.Single(result);
        Assert.Equal("TestUser", result[0].Name);
        Assert.Single(result[0].Attributes);
        Assert.Equal(NominatorAttributeType.Inquisitor, result[0].Attributes[0].AttributeName);
        Assert.Equal(new DateOnly(2020, 1, 1), result[0].Attributes[0].EffectiveAt);
        Assert.Equal(new DateOnly(2020, 2, 1), result[0].Attributes[0].EffectiveUntil);
    }

    [Fact]
    public void ParseAttributeType_WithValidRoles_ReturnsCorrectTypes()
    {
        var directives = CreateDirectivesWithBar("[[User:Test|Test]]", "01/01/2020", "end", "inq");
        using var extractor = new NominatorAttributeExtractor(directives);
        var result = extractor.Extract().First();
        Assert.Equal(NominatorAttributeType.Inquisitor, result.Attributes[0].AttributeName);

        directives = CreateDirectivesWithBar("[[User:Test|Test]]", "01/01/2020", "end", "ac");
        using var extractor2 = new NominatorAttributeExtractor(directives);
        var result2 = extractor2.Extract().First();
        Assert.Equal(NominatorAttributeType.AcMember, result2.Attributes[0].AttributeName);

        directives = CreateDirectivesWithBar("[[User:Test|Test]]", "01/01/2020", "end", "ec");
        using var extractor3 = new NominatorAttributeExtractor(directives);
        var result3 = extractor3.Extract().First();
        Assert.Equal(NominatorAttributeType.EduCorp, result3.Attributes[0].AttributeName);
    }

    [Fact]
    public void ParseAttributeType_WithInvalidRole_ThrowsException()
    {
        var directives = CreateDirectivesWithBar("[[User:Test|Test]]", "01/01/2020", "end", "invalid");
        using var extractor = new NominatorAttributeExtractor(directives);

        var exception = Assert.Throws<Exception>(() => extractor.Extract().ToList());
        Assert.Contains("Unexpected attribute name invalid", exception.Message);
    }

    [Fact]
    public void ParseNominatorName_WithValidUserLink_ReturnsCorrectName()
    {
        var directives = CreateDirectivesWithBar("[[User:TestUser|DisplayName]]", "01/01/2020", "end", "inq");
        using var extractor = new NominatorAttributeExtractor(directives);
        var result = extractor.Extract().First();

        Assert.Equal("DisplayName", result.Name);
        Assert.False(result.IsRedacted);
        Assert.Single(result.Attributes);
        Assert.Equal(NominatorAttributeType.Inquisitor, result.Attributes[0].AttributeName);
        Assert.Equal(new DateOnly(2020, 1, 1), result.Attributes[0].EffectiveAt);
        Assert.Null(result.Attributes[0].EffectiveUntil);
    }

    [Fact]
    public void ParseNominatorName_WithInvalidFormat_ThrowsException()
    {
        var directives = CreateDirectivesWithBar("InvalidUserFormat", "01/01/2020", "end", "inq");
        using var extractor = new NominatorAttributeExtractor(directives);

        var exception = Assert.Throws<Exception>(() => extractor.Extract().ToList());
        Assert.Contains("Unexpected name InvalidUserFormat", exception.Message);
    }

    [Fact]
    public void Extract_IgnoresNonBarAndNonBarsetLines()
    {
        var directives = new[]
        {
            new TimelineDirective
            {
                Identifier = "DateFormat",
                AttributeLines =
                [
                    new Dictionary<string, string> { { "dd/MM/yyyy", "" } }
                ]
            },
            new TimelineDirective
            {
                Identifier = "PlotData",
                AttributeLines =
                [
                    new Dictionary<string, string> { { "someother", "value" } },
                    new Dictionary<string, string>
                    {
                        { "bar", "[[User:TestUser|TestUser]]" },
                        { "from", "01/01/2020" },
                        { "till", "01/02/2020" },
                        { "color", "inq" },
                        { "text", "[[User:TestUser|TestUser]]" }
                    },
                    new Dictionary<string, string> { { "another", "ignored" } }
                ]
            }
        };

        using var extractor = new NominatorAttributeExtractor(directives);
        var result = extractor.Extract().ToList();

        Assert.Single(result);
        Assert.Equal("TestUser", result[0].Name);
    }

    private static TimelineDirective[] CreateDirectivesWithBar(string text, string from, string till, string color)
    {
        return new[]
        {
            new TimelineDirective
            {
                Identifier = "DateFormat",
                AttributeLines =
                [
                    new Dictionary<string, string> { { "dd/MM/yyyy", "" } }
                ]
            },
            new TimelineDirective
            {
                Identifier = "PlotData",
                AttributeLines =
                [
                    new Dictionary<string, string>
                    {
                        { "bar", text },
                        { "from", from },
                        { "till", till },
                        { "color", color },
                        { "text", text }
                    }
                ]
            }
        };
    }
}
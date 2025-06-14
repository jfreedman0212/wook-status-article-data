using WookiepediaStatusArticleData.Services.Nominators.AttributeTimeline;
using Xunit.Abstractions;

namespace WookiepediaStatusArticleData.Tests;

public class UnitTest1(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public void Test1()
    {
        using var tokenizer = new TimelineTokenizer(new StreamReader("/var/home/josh/wook_roles_timeline.txt"));
        using var parser = new TimelineParser(tokenizer.Tokenize());
        using var extractor = new NominatorAttributeExtractor(parser.Parse());

        // Access parsed data
        foreach (var nominator in extractor.Extract())
        {
            testOutputHelper.WriteLine($"Nominator: {nominator.Name}");
            foreach (var attribute in nominator.Attributes!)
            {
                testOutputHelper.WriteLine($"\t{attribute.AttributeName}: {attribute.EffectiveAt:d} - {attribute.EffectiveUntil?.ToString("d") ?? "now"}");
            }
        }
    }
}
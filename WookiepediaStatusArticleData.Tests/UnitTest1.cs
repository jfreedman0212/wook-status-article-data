using WookiepediaStatusArticleData.Services.Nominators.AttributeTimeline;
using Xunit.Abstractions;

namespace WookiepediaStatusArticleData.Tests;

public class UnitTest1(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public void Test1()
    {
        using var tokenizer = new TimelineTokenizer(new StreamReader("/var/home/josh/wook_roles_timeline.txt"));
        var tokens = tokenizer.Tokenize();
        using var parser = new TimelineParser(tokens);
        
        // Access parsed data
        foreach (var directive in parser.Parse())
        {
            testOutputHelper.WriteLine($"Directive: {directive.Identifier}");

            for (var i = 0; i < directive.AttributeLines.Count; i++)
            {
                var line = directive.AttributeLines[i];
                foreach (var attribute in line)
                {
                    testOutputHelper.WriteLine($"Line {i} Attribute: {attribute.Identifier} - {attribute.Value}");
                }
            }
        }
    }
}
using WookiepediaStatusArticleData.Services.Nominators.AttributeTimeline;

namespace WookiepediaStatusArticleData.Tests;

public class TimelineParserTest
{
    [Fact]
    public void TestSingleLineDirectiveWithKeyValuePairs()
    {
        var tokens = new[]
        {
            new TimelineToken { Type = TimelineTokenType.Identifier, Lexeme = "ImageSize" },
            new TimelineToken { Type = TimelineTokenType.Whitespace, Lexeme = " " },
            new TimelineToken { Type = TimelineTokenType.Equals, Lexeme = "=" },
            new TimelineToken { Type = TimelineTokenType.Whitespace, Lexeme = " " },
            new TimelineToken { Type = TimelineTokenType.Identifier, Lexeme = "width" },
            new TimelineToken { Type = TimelineTokenType.Colon, Lexeme = ":" },
            new TimelineToken { Type = TimelineTokenType.Identifier, Lexeme = "1300" },
            new TimelineToken { Type = TimelineTokenType.Whitespace, Lexeme = " " },
            new TimelineToken { Type = TimelineTokenType.Identifier, Lexeme = "height" },
            new TimelineToken { Type = TimelineTokenType.Colon, Lexeme = ":" },
            new TimelineToken { Type = TimelineTokenType.Identifier, Lexeme = "auto" },
            new TimelineToken { Type = TimelineTokenType.Newline, Lexeme = "\n" }
        };

        using var parser = new TimelineParser(tokens);
        var directives = parser.Parse().ToList();

        Assert.Single(directives);
        var directive = directives[0];
        
        Assert.Equal("ImageSize", directive.Identifier);
        Assert.Single(directive.AttributeLines);
        
        var attributes = directive.AttributeLines[0];
        Assert.Equal(2, attributes.Count);
        Assert.Equal("1300", attributes["width"]);
        Assert.Equal("auto", attributes["height"]);
    }

    [Fact]
    public void TestSingleLineDirectiveWithBareValues()
    {
        var tokens = new[]
        {
            new TimelineToken { Type = TimelineTokenType.Identifier, Lexeme = "Legend" },
            new TimelineToken { Type = TimelineTokenType.Whitespace, Lexeme = " " },
            new TimelineToken { Type = TimelineTokenType.Equals, Lexeme = "=" },
            new TimelineToken { Type = TimelineTokenType.Whitespace, Lexeme = " " },
            new TimelineToken { Type = TimelineTokenType.Identifier, Lexeme = "bottom" },
            new TimelineToken { Type = TimelineTokenType.Whitespace, Lexeme = " " },
            new TimelineToken { Type = TimelineTokenType.Identifier, Lexeme = "horizontal" },
            new TimelineToken { Type = TimelineTokenType.Newline, Lexeme = "\n" }
        };

        using var parser = new TimelineParser(tokens);
        var directives = parser.Parse().ToList();

        Assert.Single(directives);
        var directive = directives[0];
        
        Assert.Equal("Legend", directive.Identifier);
        Assert.Single(directive.AttributeLines);
        
        var attributes = directive.AttributeLines[0];
        Assert.Equal(2, attributes.Count);
        Assert.Equal("", attributes["bottom"]);
        Assert.Equal("", attributes["horizontal"]);
    }

    [Fact]
    public void TestMultiLineDirective()
    {
        var tokens = new[]
        {
            new TimelineToken { Type = TimelineTokenType.Identifier, Lexeme = "PlotData" },
            new TimelineToken { Type = TimelineTokenType.Whitespace, Lexeme = " " },
            new TimelineToken { Type = TimelineTokenType.Equals, Lexeme = "=" },
            new TimelineToken { Type = TimelineTokenType.Newline, Lexeme = "\n" },
            new TimelineToken { Type = TimelineTokenType.Whitespace, Lexeme = "  " },
            new TimelineToken { Type = TimelineTokenType.Identifier, Lexeme = "bar" },
            new TimelineToken { Type = TimelineTokenType.Colon, Lexeme = ":" },
            new TimelineToken { Type = TimelineTokenType.Identifier, Lexeme = "User1" },
            new TimelineToken { Type = TimelineTokenType.Whitespace, Lexeme = " " },
            new TimelineToken { Type = TimelineTokenType.Identifier, Lexeme = "from" },
            new TimelineToken { Type = TimelineTokenType.Colon, Lexeme = ":" },
            new TimelineToken { Type = TimelineTokenType.Identifier, Lexeme = "2020" },
            new TimelineToken { Type = TimelineTokenType.Newline, Lexeme = "\n" },
            new TimelineToken { Type = TimelineTokenType.Whitespace, Lexeme = "  " },
            new TimelineToken { Type = TimelineTokenType.Identifier, Lexeme = "bar" },
            new TimelineToken { Type = TimelineTokenType.Colon, Lexeme = ":" },
            new TimelineToken { Type = TimelineTokenType.Identifier, Lexeme = "User2" },
            new TimelineToken { Type = TimelineTokenType.Whitespace, Lexeme = " " },
            new TimelineToken { Type = TimelineTokenType.Identifier, Lexeme = "from" },
            new TimelineToken { Type = TimelineTokenType.Colon, Lexeme = ":" },
            new TimelineToken { Type = TimelineTokenType.Identifier, Lexeme = "2021" },
            new TimelineToken { Type = TimelineTokenType.Newline, Lexeme = "\n" }
        };

        using var parser = new TimelineParser(tokens);
        var directives = parser.Parse().ToList();

        Assert.Single(directives);
        var directive = directives[0];
        
        Assert.Equal("PlotData", directive.Identifier);
        Assert.Equal(2, directive.AttributeLines.Count);
        
        var line1 = directive.AttributeLines[0];
        Assert.Equal(2, line1.Count);
        Assert.Equal("User1", line1["bar"]);
        Assert.Equal("2020", line1["from"]);
        
        var line2 = directive.AttributeLines[1];
        Assert.Equal(2, line2.Count);
        Assert.Equal("User2", line2["bar"]);
        Assert.Equal("2021", line2["from"]);
    }

    [Fact]
    public void TestMultipleDirectives()
    {
        var tokens = new[]
        {
            new TimelineToken { Type = TimelineTokenType.Identifier, Lexeme = "ImageSize" },
            new TimelineToken { Type = TimelineTokenType.Equals, Lexeme = "=" },
            new TimelineToken { Type = TimelineTokenType.Identifier, Lexeme = "width" },
            new TimelineToken { Type = TimelineTokenType.Colon, Lexeme = ":" },
            new TimelineToken { Type = TimelineTokenType.Identifier, Lexeme = "1300" },
            new TimelineToken { Type = TimelineTokenType.Newline, Lexeme = "\n" },
            new TimelineToken { Type = TimelineTokenType.Identifier, Lexeme = "Legend" },
            new TimelineToken { Type = TimelineTokenType.Equals, Lexeme = "=" },
            new TimelineToken { Type = TimelineTokenType.Identifier, Lexeme = "position" },
            new TimelineToken { Type = TimelineTokenType.Colon, Lexeme = ":" },
            new TimelineToken { Type = TimelineTokenType.Identifier, Lexeme = "bottom" },
            new TimelineToken { Type = TimelineTokenType.Newline, Lexeme = "\n" }
        };

        using var parser = new TimelineParser(tokens);
        var directives = parser.Parse().ToList();

        Assert.Equal(2, directives.Count);
        
        var directive1 = directives[0];
        Assert.Equal("ImageSize", directive1.Identifier);
        Assert.Single(directive1.AttributeLines);
        Assert.Equal("1300", directive1.AttributeLines[0]["width"]);
        
        var directive2 = directives[1];
        Assert.Equal("Legend", directive2.Identifier);
        Assert.Single(directive2.AttributeLines);
        Assert.Equal("bottom", directive2.AttributeLines[0]["position"]);
    }

    [Fact]
    public void TestCommentsAreIgnored()
    {
        var tokens = new[]
        {
            new TimelineToken { Type = TimelineTokenType.Comment, Lexeme = " this is a comment" },
            new TimelineToken { Type = TimelineTokenType.Identifier, Lexeme = "ImageSize" },
            new TimelineToken { Type = TimelineTokenType.Equals, Lexeme = "=" },
            new TimelineToken { Type = TimelineTokenType.Comment, Lexeme = " another comment" },
            new TimelineToken { Type = TimelineTokenType.Identifier, Lexeme = "width" },
            new TimelineToken { Type = TimelineTokenType.Colon, Lexeme = ":" },
            new TimelineToken { Type = TimelineTokenType.Identifier, Lexeme = "1300" },
            new TimelineToken { Type = TimelineTokenType.Newline, Lexeme = "\n" }
        };

        using var parser = new TimelineParser(tokens);
        var directives = parser.Parse().ToList();

        Assert.Single(directives);
        var directive = directives[0];
        
        Assert.Equal("ImageSize", directive.Identifier);
        Assert.Single(directive.AttributeLines);
        Assert.Equal("1300", directive.AttributeLines[0]["width"]);
    }

    [Fact]
    public void TestInvalidDirectiveStartThrowsException()
    {
        var tokens = new[]
        {
            new TimelineToken { Type = TimelineTokenType.Equals, Lexeme = "=" },
            new TimelineToken { Type = TimelineTokenType.Identifier, Lexeme = "value" }
        };

        using var parser = new TimelineParser(tokens);
        
        var exception = Assert.Throws<Exception>(() => parser.Parse().ToList());
        Assert.Contains("Expected Identifier to start a directive", exception.Message);
    }

    [Fact]
    public void TestMissingEqualsThrowsException()
    {
        var tokens = new[]
        {
            new TimelineToken { Type = TimelineTokenType.Identifier, Lexeme = "ImageSize" },
            new TimelineToken { Type = TimelineTokenType.Identifier, Lexeme = "value" }
        };

        using var parser = new TimelineParser(tokens);
        
        var exception = Assert.Throws<Exception>(() => parser.Parse().ToList());
        Assert.Contains("Expected either whitespace or equals", exception.Message);
    }
}
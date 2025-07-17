using WookiepediaStatusArticleData.Services.Nominators.AttributeTimeline;

namespace WookiepediaStatusArticleData.Tests;

public class TimelineTokenizerTest
{
    [Fact]
    public void Test1()
    {
        var testData = "ImageSize  = width:1300 height:auto barincrement:19";
        using var memoryStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(testData));
        using var tokenizer = new TimelineTokenizer(new StreamReader(memoryStream));

        var tokens = tokenizer.Tokenize().ToList();

        Assert.Equal(15, tokens.Count);

        Assert.Equal(TimelineTokenType.Identifier, tokens[0].Type);
        Assert.Equal("ImageSize", tokens[0].Lexeme);

        Assert.Equal(TimelineTokenType.Whitespace, tokens[1].Type);
        Assert.Equal("  ", tokens[1].Lexeme);

        Assert.Equal(TimelineTokenType.Equals, tokens[2].Type);
        Assert.Equal("=", tokens[2].Lexeme);

        Assert.Equal(TimelineTokenType.Whitespace, tokens[3].Type);
        Assert.Equal(" ", tokens[3].Lexeme);

        Assert.Equal(TimelineTokenType.Identifier, tokens[4].Type);
        Assert.Equal("width", tokens[4].Lexeme);

        Assert.Equal(TimelineTokenType.Colon, tokens[5].Type);
        Assert.Equal(":", tokens[5].Lexeme);

        Assert.Equal(TimelineTokenType.Identifier, tokens[6].Type);
        Assert.Equal("1300", tokens[6].Lexeme);

        Assert.Equal(TimelineTokenType.Whitespace, tokens[7].Type);
        Assert.Equal(" ", tokens[7].Lexeme);

        Assert.Equal(TimelineTokenType.Identifier, tokens[8].Type);
        Assert.Equal("height", tokens[8].Lexeme);

        Assert.Equal(TimelineTokenType.Colon, tokens[9].Type);
        Assert.Equal(":", tokens[9].Lexeme);

        Assert.Equal(TimelineTokenType.Identifier, tokens[10].Type);
        Assert.Equal("auto", tokens[10].Lexeme);

        Assert.Equal(TimelineTokenType.Whitespace, tokens[11].Type);
        Assert.Equal(" ", tokens[11].Lexeme);

        Assert.Equal(TimelineTokenType.Identifier, tokens[12].Type);
        Assert.Equal("barincrement", tokens[12].Lexeme);

        Assert.Equal(TimelineTokenType.Colon, tokens[13].Type);
        Assert.Equal(":", tokens[13].Lexeme);

        Assert.Equal(TimelineTokenType.Identifier, tokens[14].Type);
        Assert.Equal("19", tokens[14].Lexeme);
    }

    [Fact]
    public void TestQuotedValues()
    {
        var testData = "name = \"John Doe\" age = 25";
        using var memoryStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(testData));
        using var tokenizer = new TimelineTokenizer(new StreamReader(memoryStream));

        var tokens = tokenizer.Tokenize().ToList();

        Assert.Equal(11, tokens.Count);

        Assert.Equal(TimelineTokenType.Identifier, tokens[0].Type);
        Assert.Equal("name", tokens[0].Lexeme);

        Assert.Equal(TimelineTokenType.Whitespace, tokens[1].Type);
        Assert.Equal(" ", tokens[1].Lexeme);

        Assert.Equal(TimelineTokenType.Equals, tokens[2].Type);
        Assert.Equal("=", tokens[2].Lexeme);

        Assert.Equal(TimelineTokenType.Whitespace, tokens[3].Type);
        Assert.Equal(" ", tokens[3].Lexeme);

        // Quoted value should be treated as identifier with content inside quotes
        Assert.Equal(TimelineTokenType.Identifier, tokens[4].Type);
        Assert.Equal("John Doe", tokens[4].Lexeme);

        Assert.Equal(TimelineTokenType.Whitespace, tokens[5].Type);
        Assert.Equal(" ", tokens[5].Lexeme);

        Assert.Equal(TimelineTokenType.Identifier, tokens[6].Type);
        Assert.Equal("age", tokens[6].Lexeme);

        Assert.Equal(TimelineTokenType.Whitespace, tokens[7].Type);
        Assert.Equal(" ", tokens[7].Lexeme);

        Assert.Equal(TimelineTokenType.Equals, tokens[8].Type);
        Assert.Equal("=", tokens[8].Lexeme);

        Assert.Equal(TimelineTokenType.Whitespace, tokens[9].Type);
        Assert.Equal(" ", tokens[9].Lexeme);

        Assert.Equal(TimelineTokenType.Identifier, tokens[10].Type);
        Assert.Equal("25", tokens[10].Lexeme);
    }

    [Fact]
    public void TestSingleLineComment()
    {
        var testData = "name = value # this is a comment";
        using var memoryStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(testData));
        using var tokenizer = new TimelineTokenizer(new StreamReader(memoryStream));

        var tokens = tokenizer.Tokenize().ToList();

        Assert.Equal(7, tokens.Count);

        Assert.Equal(TimelineTokenType.Identifier, tokens[0].Type);
        Assert.Equal("name", tokens[0].Lexeme);

        Assert.Equal(TimelineTokenType.Whitespace, tokens[1].Type);
        Assert.Equal(" ", tokens[1].Lexeme);

        Assert.Equal(TimelineTokenType.Equals, tokens[2].Type);
        Assert.Equal("=", tokens[2].Lexeme);

        Assert.Equal(TimelineTokenType.Whitespace, tokens[3].Type);
        Assert.Equal(" ", tokens[3].Lexeme);

        Assert.Equal(TimelineTokenType.Identifier, tokens[4].Type);
        Assert.Equal("value", tokens[4].Lexeme);

        Assert.Equal(TimelineTokenType.Whitespace, tokens[5].Type);
        Assert.Equal(" ", tokens[5].Lexeme);

        Assert.Equal(TimelineTokenType.Comment, tokens[6].Type);
        Assert.Equal(" this is a comment", tokens[6].Lexeme);
    }

    [Fact]
    public void TestSingleLineCommentWithCodeOnNextLine()
    {
        var testData = "# this is a comment\nname = value";
        using var memoryStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(testData));
        using var tokenizer = new TimelineTokenizer(new StreamReader(memoryStream));

        var tokens = tokenizer.Tokenize().ToList();

        Assert.Equal(7, tokens.Count);

        Assert.Equal(TimelineTokenType.Comment, tokens[0].Type);
        Assert.Equal(" this is a comment", tokens[0].Lexeme);

        Assert.Equal(TimelineTokenType.Newline, tokens[1].Type);
        Assert.Equal("\n", tokens[1].Lexeme);

        Assert.Equal(TimelineTokenType.Identifier, tokens[2].Type);
        Assert.Equal("name", tokens[2].Lexeme);

        Assert.Equal(TimelineTokenType.Whitespace, tokens[3].Type);
        Assert.Equal(" ", tokens[3].Lexeme);

        Assert.Equal(TimelineTokenType.Equals, tokens[4].Type);
        Assert.Equal("=", tokens[4].Lexeme);

        Assert.Equal(TimelineTokenType.Whitespace, tokens[5].Type);
        Assert.Equal(" ", tokens[5].Lexeme);

        Assert.Equal(TimelineTokenType.Identifier, tokens[6].Type);
        Assert.Equal("value", tokens[6].Lexeme);
    }

    [Fact]
    public void TestMultiLineComment()
    {
        var testData = "before #> this is a\nmulti-line comment <# after";
        using var memoryStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(testData));
        using var tokenizer = new TimelineTokenizer(new StreamReader(memoryStream));

        var tokens = tokenizer.Tokenize().ToList();

        Assert.Equal(5, tokens.Count);

        Assert.Equal(TimelineTokenType.Identifier, tokens[0].Type);
        Assert.Equal("before", tokens[0].Lexeme);

        Assert.Equal(TimelineTokenType.Whitespace, tokens[1].Type);
        Assert.Equal(" ", tokens[1].Lexeme);

        Assert.Equal(TimelineTokenType.Comment, tokens[2].Type);
        Assert.Equal(" this is a\nmulti-line comment ", tokens[2].Lexeme);

        Assert.Equal(TimelineTokenType.Whitespace, tokens[3].Type);
        Assert.Equal(" ", tokens[3].Lexeme);

        Assert.Equal(TimelineTokenType.Identifier, tokens[4].Type);
        Assert.Equal("after", tokens[4].Lexeme);
    }
}
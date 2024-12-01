namespace WookiepediaStatusArticleData.Services.Nominators.AttributeTimeline;

/// <summary>
/// Takes a list of tokens and turns them into a series of commands. Uses the following grammar:
/// Command ->
///     Identifier Whitespace* Equals Whitespace* DataLine
/// </summary>
public class TimelineParser : IDisposable
{
    private readonly PeekEnumerator<TimelineToken> _tokens;

    public TimelineParser(IEnumerable<TimelineToken> tokens)
    {
        _tokens = new PeekEnumerator<TimelineToken>(
            tokens 
                // we don't want to deal with comments here, so ignore them before
                // the `Parse` method (and any nested calls) can even access it 
                .Where(token => token.Type != TimelineTokenType.Comment)
                .GetEnumerator()
        );
    }

    public IEnumerable<TimelineDirective> Parse()
    {
        // each iteration of this loop should represent a command, which must be the start of a line.
        // assume the top of the loop (the condition) starts at the beginning of the previous row
        // (or right before the file if this is the first iteration).

        while (_tokens.MoveNext())
        {
            var currentToken = _tokens.Current;

            // since we remove comments from the enumerable, we don't need to check for them here
            switch (currentToken.Type)
            {
                case TimelineTokenType.Identifier:
                    yield return ParseDirective(currentToken);
                    break;
                case TimelineTokenType.Newline:
                    // ignore newlines at the directive level
                    continue;
                case TimelineTokenType.Whitespace:
                case TimelineTokenType.Equals:
                case TimelineTokenType.Colon:
                // whitespace, equals, colons cannot start a directive, so bail
                default:
                    // TODO: provide a more descriptive error
                    throw new Exception($"Expected Identifier to start a directive, received {currentToken.Type}");
            }
        }
    }

    private TimelineDirective ParseDirective(TimelineToken directiveIdentifier)
    {
        // make sure the next meaningful token (i.e. not whitespace) is an equal sign
        while (_tokens.MoveNext() && _tokens.Current.Type != TimelineTokenType.Equals)
        {
            if (_tokens.Current.Type != TimelineTokenType.Whitespace)
            {
                // TODO: make error more descriptive
                throw new Exception($"Expected either whitespace or equals, got {_tokens.Current.Type}");
            }
        }

        // discard the equal sign and any whitespace until we hit a non-whitespace token
        while (_tokens.MoveNext() && _tokens.Current.Type == TimelineTokenType.Whitespace) ;

        return _tokens.Current.Type switch
        {
            // if next non-whitespace token is an identifier, parse a single line of data.
            TimelineTokenType.Identifier => new TimelineDirective
            {
                Identifier = directiveIdentifier.Lexeme,
                AttributeLines = [ParseSingleLineDirectiveValue()]
            },
            // if it's a new line, parse a multi-line set of data
            TimelineTokenType.Newline => new TimelineDirective
            {
                Identifier = directiveIdentifier.Lexeme,
                AttributeLines = ParseMultiLineDirectiveValues()
            },
            // otherwise, bail. not what we expected.
            // TODO: make error message more descriptive!
            _ => throw new Exception($"Expected an identifier or a newline, got {_tokens.Current.Type} instead")
        };
    }

    private IDictionary<string, string> ParseSingleLineDirectiveValue()
    {
        var attributes = new Dictionary<string, string>();

        do
        {
            // assume the loop STARTS with an identifier as the current token
            var identifierToken = _tokens.Current;

            // it's possible we ran out after this (since we DO support bare values)
            if (!_tokens.MoveNext())
            {
                break;
            }

            // if we have a key-value pair, handle that

            switch (_tokens.Current.Type)
            {
                case TimelineTokenType.Colon:
                {
                    // make sure we have a value next (and bail if we don't)
                    if (!_tokens.MoveNext() || _tokens.Current.Type != TimelineTokenType.Identifier)
                    {
                        throw new Exception($"Expected a value after the colon, got {_tokens.Current.Type}");
                    }

                    var valueToken = _tokens.Current;

                    attributes.Add(identifierToken.Lexeme, valueToken.Lexeme);
                    break;
                }
                case TimelineTokenType.Whitespace:
                case TimelineTokenType.Newline:
                {
                    attributes.Add(identifierToken.Lexeme, "");
                    break;
                }
                default:
                    throw new Exception(
                        $"Expected a colon, whitespace, or a newline after identifier, got {_tokens.Current.Type}"
                    );
            }

            // break out of the loop now if we received a newline
            if (_tokens.Current.Type == TimelineTokenType.Newline) break;

            // consume whitespace until the next important token
            while (_tokens.MoveNext() && _tokens.Current.Type == TimelineTokenType.Whitespace);

            // make sure the loop starts (or the condition is checked) with an identifier or a newline

            if (_tokens.Current.Type is not (TimelineTokenType.Identifier or TimelineTokenType.Newline))
            {
                throw new Exception($"Expected either an identifier or a newline, got {_tokens.Current.Type}");
            }
        } while (_tokens.Current.Type != TimelineTokenType.Newline);

        return attributes;
    }

    private IList<IDictionary<string, string>> ParseMultiLineDirectiveValues()
    {
        IList<IDictionary<string, string>> lines = [];

        // assume we start each iteration at the end of the previous line and the next line starts with whitespace
        while (_tokens.Current.Type == TimelineTokenType.Newline && _tokens.Peek?.Type == TimelineTokenType.Whitespace)
        {
            // consume the newline
            _tokens.MoveNext();
            
            // consume the whitespace so the identifier is the current token in `ParseSingleLineDirectiveValue`
            if (!_tokens.MoveNext())
            {
                throw new Exception("Expected content after whitespace for data list, but file ended abruptly.");
            }

            lines.Add(ParseSingleLineDirectiveValue());
            // `ParseSingleLineDirectiveValue` leaves the new line as the current value. the
            // top of the loop will consume it and make sure we're either reading another data line
            // or a new directive
        }

        return lines;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _tokens.Dispose();
    }
}
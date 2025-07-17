namespace WookiepediaStatusArticleData.Services.Nominators.AttributeTimeline;

public class TimelineTokenizer(StreamReader reader) : IDisposable
{
    public IEnumerable<TimelineToken> Tokenize()
    {
        // while we call `StreamReader.Read` to get one character at a time, we assume the `StreamReader`
        // buffers the content in-memory. otherwise, we'd make a read syscall for EACH CHARACTER, which introduces
        // a lot of unnecessary overhead

        while (!reader.EndOfStream)
        {
            var character = Convert.ToChar(reader.Read());

            yield return character switch
            {
                '=' => new TimelineToken
                {
                    Type = TimelineTokenType.Equals,
                    Lexeme = character.ToString()
                },
                ':' => new TimelineToken
                {
                    Type = TimelineTokenType.Colon,
                    Lexeme = character.ToString()
                },
                ' ' or '\t' or '\r' => TokenizeWhitespace(character),
                '\n' => TokenizeNewlines(character),
                '#' => TokenizeComment(),
                '"' => TokenizeValueInQuotes(),
                _ => TokenizeIdentifierOrText(character)
            };
        }
    }

    private TimelineToken TokenizeNewlines(char character)
    {
        var lexeme = character.ToString();

        while (!reader.EndOfStream && Convert.ToChar(reader.Peek()) is '\n')
        {
            lexeme += Convert.ToChar(reader.Read());
        }

        return new TimelineToken
        {
            Type = TimelineTokenType.Newline,
            Lexeme = lexeme,
        };
    }

    private TimelineToken TokenizeWhitespace(char character)
    {
        var lexeme = character.ToString();

        while (!reader.EndOfStream && Convert.ToChar(reader.Peek()) is ' ' or '\t' or '\r')
        {
            lexeme += Convert.ToChar(reader.Read());
        }

        return new TimelineToken
        {
            Type = TimelineTokenType.Whitespace,
            Lexeme = lexeme,
        };
    }

    private TimelineToken TokenizeComment()
    {
        var characterAfterPound = Convert.ToChar(reader.Peek());
        var commentText = string.Empty;

        // this is a multi-line comment, read until we hit <#
        if (characterAfterPound == '>')
        {
            // discard the '>', we just care about the comment text
            reader.Read();
            // grab the rest of the comment text
            while (!reader.EndOfStream)
            {
                if (Convert.ToChar(reader.Peek()) == '<')
                {
                    var lessThan = Convert.ToChar(reader.Read());

                    if (!reader.EndOfStream && Convert.ToChar(reader.Peek()) == '#')
                    {
                        // discard the '#', we don't need it
                        reader.Read();
                        break;
                    }

                    commentText += lessThan;
                }
                else
                {
                    commentText += Convert.ToChar(reader.Read());
                }
            }
        }
        // this is a single-line comment, read until we hit a newline
        else
        {
            while (!reader.EndOfStream && Convert.ToChar(reader.Peek()) != '\n')
            {
                commentText += Convert.ToChar(reader.Read());
            }

            // don't discard the newline, will tokenize it outside of this loop
        }

        return new TimelineToken
        {
            Type = TimelineTokenType.Comment,
            Lexeme = commentText
        };
    }

    private TimelineToken TokenizeValueInQuotes()
    {
        // the leading quote won't go in the lexeme, so we can just discard it

        var quotedLexeme = string.Empty;

        while (!reader.EndOfStream && Convert.ToChar(reader.Peek()) != '"')
        {
            quotedLexeme += Convert.ToChar(reader.Read());
        }

        // discard the final quotation mark, but only if the stream didn't end abruptly.
        // if it does, we'll handle it in the lexer phase
        if (!reader.EndOfStream)
        {
            reader.Read();
        }

        return new TimelineToken
        {
            Type = TimelineTokenType.Identifier,
            Lexeme = quotedLexeme
        };
    }

    private TimelineToken TokenizeIdentifierOrText(char character)
    {
        // probably not necessary since the switch statement at this function's callsite prevents this,
        // but placed here anyway as a sanity check
        if (!IsIdentifierOrValueCharacter(character))
        {
            throw new FormatException($"Invalid character '{character}'");
        }

        var lexeme = character.ToString();

        while (!reader.EndOfStream && IsIdentifierOrValueCharacter(Convert.ToChar(reader.Peek())))
        {
            lexeme += Convert.ToChar(reader.Read());
        }

        // leave the non-identifier character in the stream so the next iteration can pick it up

        return new TimelineToken
        {
            Type = TimelineTokenType.Identifier,
            Lexeme = lexeme
        };
    }

    private static bool IsIdentifierOrValueCharacter(char character)
    {
        return character is not (' ' or '\n' or '\t' or '\r' or '"' or '=' or ':' or '#');
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        reader.Dispose();
    }
}
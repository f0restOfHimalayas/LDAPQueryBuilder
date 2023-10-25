using System.Text;

namespace LdapQueryParser;

internal class Tokenizer
{
    private readonly string _query;
    private int _currentIndex;

    private readonly Dictionary<string, TokenType> _tokensMap = new()
    {
        ["("] = TokenType.OpenParen,
        [")"] = TokenType.CloseParen,
        ["AND"] = TokenType.AND,
        ["OR"] = TokenType.OR
    };

    private readonly HashSet<string> _possibleToken = new()
        {"A", "O"};

    internal Tokenizer(string query)
    {
        _query = query;
        _currentIndex = 0;
    }

    internal Token NextToken()
    {
        var tokenStringBuilder = new StringBuilder();
        for (var i = _currentIndex; _currentIndex < _query.Length; _currentIndex++, i++)
        {
            var curr = _query[i].ToString();

            ValidateSpaceDelimiter(curr, i);

            if (_tokensMap.ContainsKey(curr) && tokenStringBuilder.Length > 0)
            {
                // We found a token but there is already a running token in buffer. No need to adjust the _currentIndex.
                // It will resume from _currentIndex in the next cycle.
                return new Token(tokenStringBuilder.ToString(), TokenType.Value, _currentIndex);
            }

            if (_tokensMap.ContainsKey(curr))
            {
                UpdateCurrentIndex(i);
                return new Token(curr, _tokensMap[curr], _currentIndex);
            }

            if (curr == " " && tokenStringBuilder.Length > 0 && _possibleToken.Contains(tokenStringBuilder[0].ToString()))
            {
                var s = tokenStringBuilder.ToString();
                if (_tokensMap.ContainsKey(s))
                {
                    UpdateCurrentIndex(i);
                    return new Token(s, _tokensMap[s], _currentIndex);
                }
            }

            if (curr == " ")
            {
                break;
            }

            // Value token
            tokenStringBuilder.Append(_query[i]);
        }

        UpdateCurrentIndex(_currentIndex);
        var tokenValue = tokenStringBuilder.ToString();
        var tokenType = string.IsNullOrWhiteSpace(tokenValue) ? TokenType.End : TokenType.Value;
        return new Token(tokenValue, tokenType, _currentIndex);
    }

    private void UpdateCurrentIndex(int currLoc)
    {
        _currentIndex = currLoc + 1;
        for (var i = _currentIndex; i < _query.Length; i++)
        {
            if (_query[i] == ' ')
            {
                ++_currentIndex;
                continue;
            }

            break;
        }
    }

    private void ValidateSpaceDelimiter(string curr, int index)
    {
        if (curr != " ")
        {
            return;
        }

        var prev = _query[index - 1];
        var next = _query[index + 1];
        if (prev == '=' || next == '=')
        {
            throw new ParserException($"space not allowed at pos: {index} in expression: {_query}");
        }
    }
}

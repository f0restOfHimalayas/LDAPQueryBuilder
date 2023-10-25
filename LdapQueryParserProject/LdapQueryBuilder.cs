using System.Text;

namespace LdapQueryParser;

public static class LdapQueryBuilder
{
    public static string Create(string query)
    {
        var tokenStack = new Stack<Token>();
        var tokenizer = new Tokenizer(query);

        Token next;
        do
        {
            next = tokenizer.NextToken();
            if (next.Type == TokenType.CloseParen)
            {
                HandleCloseParen(tokenStack);
            }
            else if (next.Type != TokenType.End)
            {
                tokenStack.Push(next);
            }
        } while (next.Type != TokenType.End);

        var expr = CollectExpression(tokenStack);

        return expr;
    }

    private static string CollectExpression(Stack<Token> tokenStack)
    {
        var collectGroup = new StringBuilder();
        var andExpression = false;
        var orExpression = false;
        var tmpStack = new Stack<string>();

        // collect all.
        while (tokenStack.Count > 0)
        {
            var pushedTokens = tokenStack.Pop();
            if (pushedTokens.Type is TokenType.OpenParen or TokenType.CloseParen)
            {
                throw new ParserException("Invalid expression ( or ) not expected");
            }

            (andExpression, orExpression, var value) = HandleTokenType(pushedTokens, andExpression, orExpression);
            if (!string.IsNullOrWhiteSpace(value))
            {
                // fix order of append here
                tmpStack.Push(value);
            }
        }

        while (tmpStack.Count > 0)
        {
            var val = tmpStack.Pop();
            collectGroup.Append(val);
        }

        if (andExpression || orExpression)
        {
            collectGroup.Insert(0, andExpression ? "&" : "|");
            collectGroup.Insert(0, '(');
            collectGroup.Append(')');
        }

        // push running result of this group back to the stack.
        return collectGroup.ToString();
    }

    private static void HandleCloseParen(Stack<Token> tokenStack)
    {
        var andToken = false;
        var orToken = false;
        var collectGroup = new StringBuilder();
        var tmpStack = new Stack<string>();

        while (tokenStack.Count > 0)
        {
            var pushedTokens = tokenStack.Pop();
            if (pushedTokens.Type == TokenType.OpenParen)
            {
                while (tmpStack.Count > 0)
                {
                    var val = tmpStack.Pop();
                    collectGroup.Append(val);
                }

                if (andToken || orToken)
                {
                    collectGroup.Insert(0, andToken ? "&" : "|");
                    collectGroup.Insert(0, '(');
                    collectGroup.Append(')');
                }

                // push running result of this group back to the stack.
                tokenStack.Push(new Token(collectGroup.ToString(), TokenType.Value, -1));
                break;
            }

            (andToken, orToken, var value) = HandleTokenType(pushedTokens, andToken, orToken);
            if (!string.IsNullOrWhiteSpace(value))
            {
                tmpStack.Push(value);
            }
        }
    }

    private static (bool andExpr, bool orExpr, string token) HandleTokenType(Token token, bool andExpression,
        bool orExpression)
    {
        var andToken = andExpression;
        var orToken = orExpression;
        var value = string.Empty;
        switch (token.Type)
        {
            case TokenType.End:
                throw new ParserException("Invalid expression: End of token not expected");
            case TokenType.OpenParen:
                throw new ParserException($"Invalid expression: `(` not expected at {token.Index}");
            case TokenType.CloseParen:
                throw new ParserException($"Invalid expression: `)` not expected at {token.Index}");
            case TokenType.AND:
                if (orExpression)
                {
                    throw new ParserException(
                        $"AND not expected in an OR expression without precedence at {token.Index}");
                }

                andToken = true;
                break;
            case TokenType.OR:
                if (andExpression)
                {
                    throw new ParserException(
                        $"OR not expected for AND expression without precedence at {token.Index}.");
                }

                orToken = true;
                break;
            case TokenType.Value:
                value = GetTokenValue(token);
                break;
            default:
                throw new ParserException($"Unknown token in expression at {token.Index}");
        }

        return (andToken, orToken, value);
    }

    private static string GetTokenValue(Token token)
    {
        var notToken = token.Value.Contains("!=");
        if (notToken)
        {
            var parts = token.Value.Split("!=");
            return $"(!({parts[0]}={parts[1]}))";
        }

        if (token.Value.StartsWith("("))
        {
            return token.Value;
        }

        return $"({token.Value})";

    }
}

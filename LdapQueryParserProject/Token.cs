namespace LdapQueryParser;

internal struct Token
{
    internal string Value { get; }
    internal TokenType Type { get; }
    internal int Index { get; }

    public Token(string value, TokenType type, int index)
    {
        Value = value;
        Type = type;
        Index = index;
    }
}

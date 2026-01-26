namespace Script.Core.Expressions.LiteralExpressions;

public abstract class UserInputExpression : Expression
{
    public string RawText { get; private set; }

    protected UserInputExpression(string raw)
    {
        RawText = raw;
        Reparse();
    }

    public void UpdateRaw(string raw)
    {
        RawText = raw;
        Reparse();
    }

    protected abstract void Reparse();
}


#nullable enable

using Script.Core.Expressions;

public interface IExpressionSlotHost
{
    string DebugName { get; }

    void SetExpression(int index, Expression? expression);
    Expression? GetExpression(int index);
}

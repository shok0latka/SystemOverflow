#nullable enable

using Script.Core.Expressions;


namespace Script.UI.Views 
{
    public interface IExpressionSlotHost
    {
        string DebugName { get; }

        void SetExpression(int index, Expression? expression);
        Expression? GetExpression(int index);
    }
}
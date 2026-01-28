using Script.Core.Expressions;

namespace Script.Core.Statements.ControlFlow;

public readonly struct ControlFlowResult
{
    public ControlFlowKind Kind { get; }
    public Expression? Expression { get; }

    private ControlFlowResult(ControlFlowKind kind, Expression? expr = null)
    {
        Kind = kind;
        Expression = expr;
    }

    public static readonly ControlFlowResult None =
        new(ControlFlowKind.None);

    public static readonly ControlFlowResult Break =
        new(ControlFlowKind.Break);

    public static readonly ControlFlowResult Continue =
        new(ControlFlowKind.Continue);

    public static ControlFlowResult Return(Expression expr) =>
        new(ControlFlowKind.Return, expr);
}

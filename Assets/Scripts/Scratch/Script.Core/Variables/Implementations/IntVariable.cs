using Script.Core.Expressions;

namespace Script.Core.Variables.Implementations;

public sealed class IntVariable() : Variable(Types.ScriptType.Integer)
{
    private int runtimeValue;

    public override object Raw
    {
        get => runtimeValue;
    }

    public override void Assign(Expression e)
    {
        runtimeValue = Convert.ToInt32(e.Evaluate());
    }
}

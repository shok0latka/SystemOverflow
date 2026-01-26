using Script.Core.Expressions;
using Script.Core.Types;

namespace Script.Core.Variables.Implementations;

public sealed class BooleanVariable(): Variable(ScriptType.Boolean)
{
    private bool runtimeValue;

    public override object Raw
    {
        get => runtimeValue;
    }

    public override void Assign(Expression e)
    {
        runtimeValue = Convert.ToBoolean(e.Evaluate());
    }
}

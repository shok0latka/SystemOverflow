#nullable enable

using System;

namespace Script.Core.Expressions.LiteralExpressions
{
    public sealed class TrueConstant : Expression
    {
        public TrueConstant()
        {
            Type = Types.ScriptType.Boolean;
        }

        public override object? Evaluate()
        {
            return true;
        }
    }

    public sealed class FalseConstant : Expression
    {
        public FalseConstant()
        {
            Type = Types.ScriptType.Boolean;
        }

        public override object? Evaluate()
        {
            return false;
        }
    }
}
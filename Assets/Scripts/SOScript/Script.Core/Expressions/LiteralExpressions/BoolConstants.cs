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

        public override int Arity()
        {
            return 0;
        }

        protected override void SetInput(int index, Expression? value)
        {
            throw new IndexOutOfRangeException();
        }

        protected override Expression? GetInput(int index)
        {
            throw new IndexOutOfRangeException();
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

        public override int Arity()
        {
            return 0;
        }

        protected override void SetInput(int index, Expression? value)
        {
            throw new IndexOutOfRangeException();
        }

        protected override Expression? GetInput(int index)
        {
            throw new IndexOutOfRangeException();
        }
    }
}
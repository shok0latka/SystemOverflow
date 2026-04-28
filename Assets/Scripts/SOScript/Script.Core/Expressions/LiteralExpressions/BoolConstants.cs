#nullable enable

using System;
using System.Threading.Tasks;

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
            InvokeOnEvaluate();
            return true;
        }

        public override async Task<object?> EvaluateAsync()
        {
            await InvokeOnEvaluateAsync();
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
            InvokeOnEvaluate();
            return false;
        }

        public override async Task<object?> EvaluateAsync()
        {
            await InvokeOnEvaluateAsync();
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
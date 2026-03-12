#nullable enable

using System;
using Script.Core.Variables;

namespace Script.Core.Expressions
{
    public class VariableExpression: Expression
    {
        public Variable Var { get; set; }

        public override object? Evaluate()
        {
            return Var.Raw;
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

        public VariableExpression(Variable v)
        {
            Var = v;
            Type = Var.Type; // Возможно будут проблемы с обновлением типа
        }
    }
}
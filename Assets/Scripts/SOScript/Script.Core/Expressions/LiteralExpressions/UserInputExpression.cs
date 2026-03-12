#nullable enable

using System;

namespace Script.Core.Expressions.LiteralExpressions
{
    public abstract class UserInputExpression : Expression
    {
        private string rawText = string.Empty;
        public string RawText 
        { 
            get => rawText;
            set
            {
                rawText = value;
                Reparse();
            } 
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

        protected abstract void Reparse();
    }
}
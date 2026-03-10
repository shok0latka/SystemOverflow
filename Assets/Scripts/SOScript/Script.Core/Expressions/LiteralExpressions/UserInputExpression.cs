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

        protected abstract void Reparse();
    }
}
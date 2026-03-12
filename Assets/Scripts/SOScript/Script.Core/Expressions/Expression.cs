#nullable enable

using System;
using Script.Core.Types;

namespace Script.Core.Expressions
{
    public abstract class Expression
    {
        private ScriptType type;

        public ScriptType Type
        {
            get => type;
            set
            {
                if (value != type)
                {
                    type = value;
                    Parent?.UpdateTypes();
                }
            }
        }
        public Expression? Parent { get; set; } = null;

        public virtual void UpdateTypes()
        {
            
        }

        public abstract object? Evaluate();

        public abstract int Arity();

        protected abstract void SetInput(int index, Expression? value);

        protected abstract Expression? GetInput(int index);

        public Expression? this[int index]
        {
            get => GetInput(index);
            set => SetInput(index, value);
        }
    }
}
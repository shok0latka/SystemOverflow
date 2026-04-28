#nullable enable

using System;
using System.Threading.Tasks;
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

        public event Func<Task>? OnEvaluateAsync;

        public virtual void UpdateTypes()
        {
            
        }

        public abstract object? Evaluate();

        public abstract Task<object?> EvaluateAsync();

        public abstract int Arity();

        protected abstract void SetInput(int index, Expression? value);

        protected abstract Expression? GetInput(int index);

        public Expression? this[int index]
        {
            get => GetInput(index);
            set => SetInput(index, value);
        }

        public Expression RegisterEvaluateCallback(Func<Task> callback)
        {
            OnEvaluateAsync += callback;
            return this;
        }

        protected void InvokeOnEvaluate()
        {
            // OnEvaluateAsync?.Invoke();
        }

        protected async Task InvokeOnEvaluateAsync()
        {
            if (OnEvaluateAsync != null)
                await OnEvaluateAsync();
        }
    }
}
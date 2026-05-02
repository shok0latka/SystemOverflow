#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Script.Core.Expressions;
using Script.Core.Types;

namespace Script.Core.Statements
{
    public class StatementArgument: Expression
    {
        private Expression? attached;

        public string Name { get; private set; }
        public List<ScriptType> AllowedTypes { get; private set; }
        public Expression? Attached
        {
            get => attached;
            set
            {
                var newType = value?.Type ?? ScriptType.Undefined;
                if (!ValidateType(newType))
                {
                    throw new ArgumentException($"Incorrect argument type {newType} for parameter '{Name}'");
                }
                attached = value;
                if (attached is not null)
                {
                    attached.Parent = this;
                    Type = attached.Type;
                }
                else
                {
                    Type = ScriptType.Undefined;
                }
            }
        }

        public StatementArgument(string name, List<ScriptType> allowedTypes)
        {
            Name = name;
            AllowedTypes = allowedTypes;
            Type = ScriptType.Undefined;
        }

        public bool ValidateType(ScriptType type, bool inRuntime = false)
        {
            return (!inRuntime && type == ScriptType.Undefined) || AllowedTypes.Contains(type);
        }

        public override object? Evaluate()
        {
            if (Attached is null)
            {
                throw new ArgumentNullException(nameof(Attached), $"Required parameter '{Name}' is empty");
            }
            if (!ValidateType(Attached.Type, true))
            {
                throw new ArgumentException($"Incorrect argument type {Attached.Type} for parameter '{Name}'");
            }

            return Attached.Evaluate();
        }

        public override Task<object?> EvaluateAsync()
        {
            if (Attached is null)
            {
                throw new ArgumentNullException(nameof(Attached), $"Required parameter '{Name}' is empty");
            }
            if (!ValidateType(Attached.Type, true))
            {
                throw new ArgumentException($"Incorrect argument type {Attached.Type} for parameter '{Name}'");
            }

            return Attached.EvaluateAsync();
        }

        public override int Arity()
        {
            return 1;
        }

        protected override void SetInput(int index, Expression? value)
        {
            if (index != 0)
            {
                throw new IndexOutOfRangeException();
            }
            Attached = value;
        }

        protected override Expression? GetInput(int index)
        {
            if (index != 0)
            {
                throw new IndexOutOfRangeException();
            }
            return Attached;
        }
    }
}
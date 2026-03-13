#nullable enable

using System;
using Script.Core.Expressions;
using Script.Core.Types;

namespace Script.Core.Variables.Implementations
{
    public sealed class BooleanVariable: Variable
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

        public BooleanVariable(string name): base(ScriptType.Boolean, name)
        {
            
        }
    }
}
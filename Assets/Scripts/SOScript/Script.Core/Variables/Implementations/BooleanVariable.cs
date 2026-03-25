#nullable enable

using System;
using System.Threading.Tasks;
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

        public override async Task AssignAsync(Expression e)
        {
            var result = await e.EvaluateAsync();
            runtimeValue = Convert.ToBoolean(result);
        }

        public BooleanVariable(string name): base(ScriptType.Boolean, name)
        {
            
        }
    }
}
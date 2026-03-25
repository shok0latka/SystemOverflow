#nullable enable

using System;
using System.Threading.Tasks;
using Script.Core.Expressions;

namespace Script.Core.Variables.Implementations
{
    public sealed class IntVariable : Variable
    {
        private int runtimeValue;

        public override object Raw
        {
            get => runtimeValue;
        }

        public override void Assign(Expression e)
        {
            runtimeValue = Convert.ToInt32(e.Evaluate());
        }

        public override async Task AssignAsync(Expression e)
        {
            var result = await e.EvaluateAsync();
            runtimeValue = Convert.ToInt32(result);
        }

        public IntVariable(string name): base(Types.ScriptType.Integer, name)
        {
            
        }
    }
}
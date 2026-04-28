#nullable enable

using System;
using System.Threading.Tasks;
using Script.Core.Expressions;

namespace Script.Core.Variables.Implementations
{
    public sealed class StringVariable : Variable
    {
        private string runtimeValue = string.Empty;

        public override object Raw
        {
            get => runtimeValue;
        }

        public override void Assign(Expression e)
        {
            runtimeValue = Convert.ToString(e.Evaluate()) ?? string.Empty;
        }

        public override async Task AssignAsync(Expression e)
        {
            var result = await e.EvaluateAsync();
            runtimeValue = Convert.ToString(result) ?? string.Empty;
        }

        public StringVariable(string name): base(Types.ScriptType.String, name)
        {
            
        }
    }
}
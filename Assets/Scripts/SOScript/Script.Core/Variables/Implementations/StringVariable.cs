#nullable enable

using System;
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

        public StringVariable(string name): base(Types.ScriptType.String, name)
        {
            
        }
    }
}
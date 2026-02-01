using Script.Core.Expressions;
using Script.Core.Types;

namespace Script.Core.Variables.Implementations
{
    public sealed class FloatVariable : Variable
    {
        private float runtimeValue;

        public override object Raw
        {
            get => runtimeValue;
        }

        public override void ValidateType(ScriptType type)
        {
            if (type != ScriptType.Undefined && type != Type && type != ScriptType.Integer)
            {
                throw new ArgumentException($"Incorrect argument type: {type}. Expected: {Type} or {ScriptType.Integer}");
            }
        }

        public override void Assign(Expression e)
        {
            runtimeValue = Convert.ToSingle(e.Evaluate());
        }

        public FloatVariable(): base(ScriptType.Float)
        {

        }
    }
}
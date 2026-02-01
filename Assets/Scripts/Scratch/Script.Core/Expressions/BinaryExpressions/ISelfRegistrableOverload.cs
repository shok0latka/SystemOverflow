using Script.Core.Types;

namespace Script.Core.Expressions.BinaryExpressions
{
    public interface ISelfRegistrableOverload
    {
        static abstract void Register(ref Dictionary<(ScriptType, ScriptType), BinaryOperatorOverload> overloads);
    }
}
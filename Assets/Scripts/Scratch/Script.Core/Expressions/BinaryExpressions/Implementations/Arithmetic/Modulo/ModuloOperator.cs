namespace Script.Core.Expressions.BinaryExpressions.Implementations.Arithmetic.Modulo
{
    public abstract class ModuloOperator : BinaryOperatorOverload, ITaggedBinaryOperator
    {
        public static BinaryOperatorTag Tag => BinaryOperatorTag.Reminder;
    }
}
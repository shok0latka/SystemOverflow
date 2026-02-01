namespace Script.Core.Expressions.BinaryExpressions.Implementations.Arithmetic.Multiplication
{
    public abstract class MultiplicationOperator : BinaryOperatorOverload, ITaggedBinaryOperator
    {
        public static BinaryOperatorTag Tag => BinaryOperatorTag.Multiplication;
    }
}
namespace Script.Core.Expressions.BinaryExpressions.Implementations.Arithmetic.Division
{
    public abstract class DivisionOperator : BinaryOperatorOverload, ITaggedBinaryOperator
    {
        public static BinaryOperatorTag Tag => BinaryOperatorTag.Division;
    }
}
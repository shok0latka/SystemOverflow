namespace Script.Core.Expressions.BinaryExpressions.Implementations.Arithmetic.Subtraction;

public abstract class SubtractionOperator : BinaryOperatorOverload, ITaggedBinaryOperator
{
    public static BinaryOperatorTag Tag => BinaryOperatorTag.Subtraction;
}

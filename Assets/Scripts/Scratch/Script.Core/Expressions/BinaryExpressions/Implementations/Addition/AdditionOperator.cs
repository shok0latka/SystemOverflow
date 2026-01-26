namespace Script.Core.Expressions.BinaryExpressions.Implementations.Addition;

public abstract class AdditionOperator : BinaryOperatorOverload, ITaggedBinaryOperator
{
    public static BinaryOperatorTag Tag => BinaryOperatorTag.Addition;
}

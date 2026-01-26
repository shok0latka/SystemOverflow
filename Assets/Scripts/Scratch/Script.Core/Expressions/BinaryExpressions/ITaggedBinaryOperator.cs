namespace Script.Core.Expressions.BinaryExpressions;

public interface ITaggedBinaryOperator
{
    static abstract BinaryOperatorTag Tag { get; }
}

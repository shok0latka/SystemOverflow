#nullable enable

using System;

namespace Script.Core.Expressions.BinaryExpressions.Implementations.Arithmetic.Multiplication
{
    public abstract class MultiplicationOperator : BinaryOperatorOverload, ITaggedBinaryOperator
    {
        public BinaryOperatorTag Tag => BinaryOperatorTag.Multiplication;
    }
}
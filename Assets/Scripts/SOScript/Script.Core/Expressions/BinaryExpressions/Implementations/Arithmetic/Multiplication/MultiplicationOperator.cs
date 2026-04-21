#nullable enable

using System;

namespace Script.Core.Expressions.BinaryExpressions.Arithmetic
{
    public abstract class MultiplicationOperator : BinaryOperatorOverload, ITaggedBinaryOperator
    {
        public BinaryOperatorTag Tag => BinaryOperatorTag.Multiplication;
    }
}

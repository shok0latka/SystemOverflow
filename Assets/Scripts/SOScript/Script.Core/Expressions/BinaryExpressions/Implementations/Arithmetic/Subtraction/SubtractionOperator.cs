#nullable enable

using System;

namespace Script.Core.Expressions.BinaryExpressions.Arithmetic
{
    public abstract class SubtractionOperator : BinaryOperatorOverload, ITaggedBinaryOperator
    {
        public BinaryOperatorTag Tag => BinaryOperatorTag.Subtraction;
    }
}

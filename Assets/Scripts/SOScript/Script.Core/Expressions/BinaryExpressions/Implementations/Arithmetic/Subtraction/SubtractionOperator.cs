#nullable enable

using System;

namespace Script.Core.Expressions.BinaryExpressions.Implementations.Arithmetic.Subtraction
{
    public abstract class SubtractionOperator : BinaryOperatorOverload, ITaggedBinaryOperator
    {
        public BinaryOperatorTag Tag => BinaryOperatorTag.Subtraction;
    }
}
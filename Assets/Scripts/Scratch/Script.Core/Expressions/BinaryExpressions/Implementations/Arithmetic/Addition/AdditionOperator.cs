#nullable enable

using System;

namespace Script.Core.Expressions.BinaryExpressions.Implementations.Arithmetic.Addition
{
    public abstract class AdditionOperator : BinaryOperatorOverload, ITaggedBinaryOperator
    {
        public BinaryOperatorTag Tag => BinaryOperatorTag.Addition;
    }
}
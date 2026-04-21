#nullable enable

using System;

namespace Script.Core.Expressions.BinaryExpressions.Arithmetic
{
    public abstract class AdditionOperator : BinaryOperatorOverload, ITaggedBinaryOperator
    {
        public BinaryOperatorTag Tag => BinaryOperatorTag.Addition;
    }
}

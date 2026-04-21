#nullable enable

using System;

namespace Script.Core.Expressions.BinaryExpressions.Arithmetic
{
    public abstract class DivisionOperator : BinaryOperatorOverload, ITaggedBinaryOperator
    {
        public BinaryOperatorTag Tag => BinaryOperatorTag.Division;
    }
}

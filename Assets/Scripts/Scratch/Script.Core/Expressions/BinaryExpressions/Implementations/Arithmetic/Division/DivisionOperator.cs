#nullable enable

using System;

namespace Script.Core.Expressions.BinaryExpressions.Implementations.Arithmetic.Division
{
    public abstract class DivisionOperator : BinaryOperatorOverload, ITaggedBinaryOperator
    {
        public BinaryOperatorTag Tag => BinaryOperatorTag.Division;
    }
}
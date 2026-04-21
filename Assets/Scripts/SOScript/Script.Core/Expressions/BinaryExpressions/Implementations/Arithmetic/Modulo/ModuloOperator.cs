#nullable enable

using System;

namespace Script.Core.Expressions.BinaryExpressions.Arithmetic
{
    public abstract class ModuloOperator : BinaryOperatorOverload, ITaggedBinaryOperator
    {
        public BinaryOperatorTag Tag => BinaryOperatorTag.Reminder;
    }
}

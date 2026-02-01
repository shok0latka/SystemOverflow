#nullable enable

using System;

namespace Script.Core.Expressions.BinaryExpressions.Implementations.Arithmetic.Modulo
{
    public abstract class ModuloOperator : BinaryOperatorOverload, ITaggedBinaryOperator
    {
        public BinaryOperatorTag Tag => BinaryOperatorTag.Reminder;
    }
}
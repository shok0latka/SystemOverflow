#nullable enable

using System;
using Script.Core.Types;

namespace Script.Core.Expressions.BinaryExpressions.Implementations.Comparison.LessThan
{
    public abstract class LessThanOperator : BinaryOperatorOverload, ITaggedBinaryOperator
    {
        public BinaryOperatorTag Tag => BinaryOperatorTag.LessThan;

        protected LessThanOperator()
        {
            ResultType = ScriptType.Boolean;
        }
    }
}
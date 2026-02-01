#nullable enable

using System;
using Script.Core.Types;

namespace Script.Core.Expressions.BinaryExpressions.Implementations.Comparison.GreaterThan
{
    public abstract class GreaterThanOperator : BinaryOperatorOverload, ITaggedBinaryOperator
    {
        public BinaryOperatorTag Tag => BinaryOperatorTag.GreaterThan;

        protected GreaterThanOperator()
        {
            ResultType = ScriptType.Boolean;
        }
    }
}
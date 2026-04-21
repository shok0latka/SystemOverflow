#nullable enable

using System;
using Script.Core.Types;

namespace Script.Core.Expressions.BinaryExpressions.Comparison
{
    public abstract class GreaterOrEqualOperator : BinaryOperatorOverload, ITaggedBinaryOperator
    {
        public BinaryOperatorTag Tag => BinaryOperatorTag.GreaterOrEqual;

        protected GreaterOrEqualOperator()
        {
            ResultType = ScriptType.Boolean;
        }
    }
}

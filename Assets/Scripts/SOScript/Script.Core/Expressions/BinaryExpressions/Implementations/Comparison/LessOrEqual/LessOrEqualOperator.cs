#nullable enable

using System;
using Script.Core.Types;

namespace Script.Core.Expressions.BinaryExpressions.Comparison
{
    public abstract class LessOrEqualOperator : BinaryOperatorOverload, ITaggedBinaryOperator
    {
        public BinaryOperatorTag Tag => BinaryOperatorTag.LessOrEqual;

        protected LessOrEqualOperator()
        {
            ResultType = ScriptType.Boolean;
        }
    }
}

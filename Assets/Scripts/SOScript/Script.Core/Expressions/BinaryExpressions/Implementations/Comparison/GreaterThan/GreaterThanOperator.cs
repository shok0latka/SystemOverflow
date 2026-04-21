#nullable enable

using System;
using Script.Core.Types;

namespace Script.Core.Expressions.BinaryExpressions.Comparison
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

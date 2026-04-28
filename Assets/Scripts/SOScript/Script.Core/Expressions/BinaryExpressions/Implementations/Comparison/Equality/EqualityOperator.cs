#nullable enable

using System;
using Script.Core.Types;

namespace Script.Core.Expressions.BinaryExpressions.Comparison
{
    public abstract class EqualityOperator : BinaryOperatorOverload, ITaggedBinaryOperator
    {
        public BinaryOperatorTag Tag => BinaryOperatorTag.Equal;

        protected EqualityOperator()
        {
            ResultType = ScriptType.Boolean;
        }
    }
}

#nullable enable

using System;

namespace Script.Core.Expressions.BinaryExpressions
{
    public interface ITaggedBinaryOperator
    {
        BinaryOperatorTag Tag { get; }
    }
}
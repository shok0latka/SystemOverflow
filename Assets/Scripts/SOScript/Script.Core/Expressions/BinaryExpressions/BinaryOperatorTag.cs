#nullable enable

using System;

namespace Script.Core.Expressions.BinaryExpressions
{
    public enum BinaryOperatorTag
    {
        Addition,
        Subtraction,
        Multiplication,
        Division,
        Reminder,
        LogicalAnd,
        LogicalOr,
        LogicalXor,
        GreaterThan,
        GreaterOrEqual,
        LessThan,
        LessOrEqual,
        Equal,
        NotEqual
    }
}
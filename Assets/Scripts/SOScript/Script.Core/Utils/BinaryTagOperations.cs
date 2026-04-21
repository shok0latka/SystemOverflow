using System;
using Script.Core.Expressions.BinaryExpressions;

namespace Script.Core.Utils
{
    public static class BinaryTagOperations
    {
        public static string GetOperatorText(BinaryOperatorTag tag)
        {
            return tag switch
            {
                BinaryOperatorTag.Addition => "+",
                BinaryOperatorTag.Subtraction => "-",
                BinaryOperatorTag.Multiplication => "·",
                BinaryOperatorTag.Division => "÷",
                BinaryOperatorTag.Reminder => "%",
                BinaryOperatorTag.LogicalAnd => "and",
                BinaryOperatorTag.LogicalOr => "or",
                BinaryOperatorTag.LogicalXor => "xor",
                BinaryOperatorTag.GreaterThan => ">",
                BinaryOperatorTag.GreaterOrEqual => "≥",
                BinaryOperatorTag.LessThan => "<",
                BinaryOperatorTag.LessOrEqual => "≤",
                BinaryOperatorTag.Equal => "=",
                BinaryOperatorTag.NotEqual => "≠",
                _ => "?"
            };
        }
    }
}


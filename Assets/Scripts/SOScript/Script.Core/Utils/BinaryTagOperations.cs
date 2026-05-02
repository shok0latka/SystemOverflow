using System;
using System.Collections.Generic;
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
    
        public static List<BinaryOperatorTag> Arithmetics => new()
        {
            BinaryOperatorTag.Addition,
            BinaryOperatorTag.Subtraction,
            BinaryOperatorTag.Multiplication,
            BinaryOperatorTag.Division,
            BinaryOperatorTag.Reminder
        };

        public static List<BinaryOperatorTag> Comparison => new()
        {
            BinaryOperatorTag.Equal,
            BinaryOperatorTag.GreaterOrEqual,
            BinaryOperatorTag.GreaterThan,
            BinaryOperatorTag.NotEqual,
            BinaryOperatorTag.LessOrEqual,
            BinaryOperatorTag.LessThan
        };
    }
}


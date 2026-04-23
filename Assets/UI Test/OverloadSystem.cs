using System;
using Script.Core.Expressions.BinaryExpressions;
using Script.Core.Expressions.BinaryExpressions.Arithmetic;
using Script.Core.Expressions.BinaryExpressions.Comparison;
using UnityEngine;

[CreateAssetMenu]
public class OverloadSystem: ScriptableObject
{
    #region  Arithmetic

    public BinaryOperatorOverloadSystem<AdditionOperator> Add {get; } = new();
    public BinaryOperatorOverloadSystem<SubtractionOperator> Sub {get; } = new();
    public BinaryOperatorOverloadSystem<MultiplicationOperator> Mul {get; } = new();
    public BinaryOperatorOverloadSystem<DivisionOperator> Div {get; } = new();
    public BinaryOperatorOverloadSystem<ModuloOperator> Rem {get; } = new();

    #endregion // Arithmetic

    #region  Comparison

    public BinaryOperatorOverloadSystem<EqualityOperator> Eq { get; } = new();
    public BinaryOperatorOverloadSystem<GreaterOrEqualOperator> Ge { get; } = new();
    public BinaryOperatorOverloadSystem<GreaterThanOperator> Gt { get; } = new();
    public BinaryOperatorOverloadSystem<NotEqualOperator> Ne { get; } = new();
    public BinaryOperatorOverloadSystem<LessOrEqualOperator> Le { get; } = new();
    public BinaryOperatorOverloadSystem<LessThanOperator> Lt { get; } = new();

    #endregion // Comparison

    public BinaryOperatorOverloadSystem this[BinaryOperatorTag tag]
    {
        get 
        {
            return tag switch
            {
                BinaryOperatorTag.Addition => Add,
                BinaryOperatorTag.Subtraction => Sub,
                BinaryOperatorTag.Division => Div,
                BinaryOperatorTag.Multiplication => Mul,
                BinaryOperatorTag.Reminder => Rem,
                
                BinaryOperatorTag.Equal => Eq,
                BinaryOperatorTag.GreaterOrEqual => Ge,
                BinaryOperatorTag.GreaterThan => Gt,
                BinaryOperatorTag.NotEqual => Ne,
                BinaryOperatorTag.LessOrEqual => Le,
                BinaryOperatorTag.LessThan => Lt,

                _ => throw new NotImplementedException()
            };
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using Script.Core.Expressions;
using Script.Core.Expressions.BinaryExpressions;
using Script.Core.Expressions.BinaryExpressions.Implementations.Arithmetic.Addition;
using Script.Core.Expressions.BinaryExpressions.Implementations.Arithmetic.Division;
using Script.Core.Expressions.BinaryExpressions.Implementations.Arithmetic.Modulo;
using Script.Core.Expressions.BinaryExpressions.Implementations.Arithmetic.Multiplication;
using Script.Core.Expressions.BinaryExpressions.Implementations.Comparison.Equality;
using Script.Core.Expressions.BinaryExpressions.Implementations.Comparison.LessOrEqual;
using Script.Core.Expressions.LiteralExpressions;
using Script.Core.Expressions.LiteralExpressions.Implementations;
using Script.Core.Statements;
using Script.Core.Variables;
using Script.Core.Variables.Implementations;
using Script.Core.Statements.ControlFlow;

using UnityEngine;


public class Test : MonoBehaviour
{
    public string NValue;

    // Start is called before the first frame update
    void Start()
    {
        var mulSystem = new BinaryOperatorOverloadSystem<MultiplicationOperator>();
        var remSystem = new BinaryOperatorOverloadSystem<ModuloOperator>();
        var lessOrEqualSystem = new BinaryOperatorOverloadSystem<LessOrEqualOperator>();
        var equalSystem = new BinaryOperatorOverloadSystem<EqualityOperator>();
        var divisionSystem = new BinaryOperatorOverloadSystem<DivisionOperator>();
        var additionSystem = new BinaryOperatorOverloadSystem<AdditionOperator>();

        var n = new IntVariable("n");
        var i = new IntVariable("i");
        var isPrime = new BooleanVariable("isPrime");

        var n_expr = new VariableExpression(n);
        var i_expr = new VariableExpression(i);
        var isPrime_expr = new VariableExpression(isPrime);

        var loop = new WhileStatement()
        {
            Condition = new BinaryExpression(lessOrEqualSystem)
            {
                LeftArg = new BinaryExpression(mulSystem)
                {
                    LeftArg = i_expr,
                    RightArg = i_expr
                },
                RightArg = n_expr
            },

            Body =
                new IfStatement()
                {
                    Condition = new BinaryExpression(equalSystem)
                    {
                        LeftArg = new BinaryExpression(remSystem)
                        {
                            LeftArg = n_expr,
                            RightArg = i_expr
                        },
                        RightArg = new NumeralExpression() { RawText = "0" }
                    },

                    Do =
                        new PrintStatement()
                        {
                            Value = new LiteralExpression() { RawText = "N is not prime: " }
                        }
                        .Then(new PrintStatement() { Value = n_expr })
                        .Then(new PrintStatement() { Value = new LiteralExpression() { RawText = " = " } })
                        .Then(new PrintStatement() { Value = i_expr })
                        .Then(new PrintStatement() { Value = new LiteralExpression() { RawText = " * " } })
                        .Then(new PrintStatement()
                        {
                            Value = new BinaryExpression(divisionSystem)
                            {
                                LeftArg = n_expr,
                                RightArg = i_expr
                            }
                        })
                        .Then(new AssignStatement(isPrime) { ToAssign = new FalseConstant() })
                        .Then(new BreakStatement())
                }
                .Then(
                    new AssignStatement(i)
                    {
                        ToAssign = new BinaryExpression(additionSystem)
                        {
                            LeftArg = i_expr,
                            RightArg = new NumeralExpression() { RawText = "1" }
                        }
                    }
                )
        };

        var program =
            new AssignStatement(i)
            {
                ToAssign = new NumeralExpression() { RawText = "2" }
            }
            .Then(new AssignStatement(isPrime) { ToAssign = new TrueConstant() })
            .Then(loop)
            .Then(
                new IfStatement()
                {
                    Condition = isPrime_expr,
                    Do =
                        new PrintStatement()
                        {
                            Value = new LiteralExpression() { RawText = "N is prime: " }
                        }
                        .Then(new PrintStatement() { Value = n_expr })
                }
            );

        n.Assign(new NumeralExpression() { RawText = NValue });

        program.Execute();
    }
}

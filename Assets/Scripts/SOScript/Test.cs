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

        var n = new IntVariable();
        var n_expr = new VariableExpression(n);

        var i = new IntVariable();

        var i_expr = new VariableExpression(i);

        var i_sqr = new BinaryExpression(mulSystem) { LeftArg = i_expr, RightArg = i_expr };
        var nRemI = new BinaryExpression(remSystem) { LeftArg = n_expr, RightArg = i_expr };

        var isPrime = new BooleanVariable();
        var isPrime_expr = new VariableExpression(isPrime);

        var loopCond = new BinaryExpression(lessOrEqualSystem) { LeftArg = i_sqr, RightArg = n_expr };
        var notPrimeCond = new BinaryExpression(equalSystem) { LeftArg = nRemI, RightArg = new NumeralExpression() { RawText = "0" }};


        var notPrimeBody = new SequenceStatement();
        notPrimeBody.Add(new PrintStatement() { Value = new LiteralExpression() { RawText = "N is not prime: " }});
        notPrimeBody.Add(new PrintStatement() { Value = n_expr });
        notPrimeBody.Add(new PrintStatement() { Value = new LiteralExpression() { RawText = " = " }});
        notPrimeBody.Add(new PrintStatement() { Value = i_expr });
        notPrimeBody.Add(new PrintStatement() { Value = new LiteralExpression() { RawText = " * " }});
        notPrimeBody.Add(new PrintStatement() { Value = new BinaryExpression(divisionSystem) { LeftArg = n_expr, RightArg = i_expr }});
        notPrimeBody.Add(new AssignStatement() { Var = isPrime, ToAssign = new FalseConstant() });
        notPrimeBody.Add(new BreakStatement());

        var loopBody = new SequenceStatement();

        loopBody.Add(new IfStatement() { Condition = notPrimeCond, Then = notPrimeBody });
        loopBody.Add(new AssignStatement() { Var = i, ToAssign = new BinaryExpression(additionSystem) { LeftArg = i_expr, RightArg = new NumeralExpression() { RawText = "1" }}});

        var loop = new WhileStatement() { Condition = loopCond, Body = loopBody };

        var primeBody = new SequenceStatement();
        primeBody.Add(new PrintStatement() { Value = new LiteralExpression() { RawText = "N is prime: " }});
        primeBody.Add(new PrintStatement() { Value = n_expr });

        var isPrimeFunction = new SequenceStatement();
        isPrimeFunction.Add(new AssignStatement() { Var = i, ToAssign = new NumeralExpression() { RawText = "2" }});
        isPrimeFunction.Add(new AssignStatement() { Var = isPrime, ToAssign = new TrueConstant() });
        isPrimeFunction.Add(loop);
        isPrimeFunction.Add(new IfStatement() { Condition = isPrime_expr, Then = primeBody });

        n.Assign(new NumeralExpression() { RawText = NValue });
        isPrimeFunction.Execute();

    }
}

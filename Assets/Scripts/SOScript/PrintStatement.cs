using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Script.Core.Expressions;
using Script.Core.Statements;
using Script.Core.Statements.ControlFlow;

public class PrintStatement: IStatement
{
    public Expression? Value { get; set; }

    public ControlFlowResult Execute()
    {
        Debug.Log(Value?.Evaluate() ?? "[null]");
        return ControlFlowResult.None;
    }
}

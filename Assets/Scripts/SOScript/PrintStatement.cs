#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Script.Core.Expressions;
using Script.Core.Statements;
using Script.Core.Statements.ControlFlow;
using Script.Core.Types;
using System.Threading.Tasks;

public class PrintStatement: IStatement
{
    public event Func<Task>? OnExecuteAsync;

    public List<StatementArgument> Arguments { get; } = new() { 
            new StatementArgument("value", new List<ScriptType> { 
                ScriptType.Boolean, ScriptType.Integer, ScriptType.Float, ScriptType.String 
            }) 
        };

    public Expression? Value
    {
        get => Arguments[0].Attached;
        set => Arguments[0].Attached = value;
    }

    IReadOnlyList<StatementArgument> IStatement.Arguments => Arguments;

    public IStatement? Next { get; set; }

    public string Name => "Print";

    public ControlFlowResult Execute()
    {
        Debug.Log(Value?.Evaluate() ?? "[null]");
        return Next?.Execute() ?? ControlFlowResult.None;
    }

    public async Task<ControlFlowResult> ExecuteAsync()
    {
        if (OnExecuteAsync != null)
            await OnExecuteAsync();

        var value = await (Value?.EvaluateAsync() ?? Task.FromResult<object?>(null));
        Debug.Log(value ?? "[null]");

        return await (Next?.ExecuteAsync() ?? Task.FromResult(ControlFlowResult.None));
    }
}

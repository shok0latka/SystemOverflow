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
using UnityEngine.UIElements;
using Script.UI.Views;
using Script.UI.Controllers;

public class PrintStatement: IStatement
{
    public event Func<Task>? OnExecuteAsync;
    
    private readonly MessageType messageType;
    private readonly UIConsole console;

    public PrintStatement(UIConsole console_, MessageType messageType_)
    {
        console = console_;
        messageType = messageType_;
    }

    public List<StatementArgument> Arguments { get; } = new() { 
            new StatementArgument("Value", new List<ScriptType> { 
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

    public string Name => $"Print {messageType}";

    public ControlFlowResult Execute()
    {
        console.Write(Value?.Evaluate()?.ToString() ?? "[null]", messageType);
        return Next?.Execute() ?? ControlFlowResult.None;
    }

    public async Task<ControlFlowResult> ExecuteAsync()
    {
        if (OnExecuteAsync != null)
            await OnExecuteAsync();

        var value = await (Value?.EvaluateAsync() ?? Task.FromResult<object?>(null));
        console.Write(value?.ToString() ?? "[null]", messageType);

        return await (Next?.ExecuteAsync() ?? Task.FromResult(ControlFlowResult.None));
    }
}

public class PrintStatementBlockView: StatementBlockView
{
    public PrintStatementBlockView(PrintStatement stmt, string? debugName = null) : base(stmt, debugName)
    {
        BuildNextSlot();
    }
}

public class PrintStatementBlockSpawner: VisualElement
{
    public PrintStatementBlockSpawner(UIConsole console, MessageType type, VisualElement editor)
    {
        var field = editor.Q("Field");
        var graph = field.Q<GraphRoot>();
        AddToClassList("stmt-block");

        RegisterCallback<ClickEvent>(evt =>
        {
            evt.StopPropagation();
            var expr = new PrintStatementBlockView(new PrintStatement(console, type));

            var editorCenter = new Vector2(
                editor.layout.width * 0.5f,
                editor.layout.height * 0.5f
            );

            var centerInField = editor.ChangeCoordinatesTo(field, editorCenter);

            Debug.Log($"Spawn print block to position: {centerInField}");
            graph.AddFreeBlock(expr, centerInField);
        });

        var title = new Label($"Print {type}");
        title.AddToClassList("stmt-title");
        Add(title);
    }
}
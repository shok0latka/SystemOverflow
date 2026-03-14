#nullable enable

using System;
using System.Collections.Generic;
using Script.Core.Expressions;
using Script.Core.Statements;
using UnityEngine.UIElements;
using UnityEngine;
using System.Threading.Tasks;

public class StatementBlockView : VisualElement, IExpressionSlotHost
{
    public IStatement Statement { get; }
    public string DebugName { get; }

    public List<ExprSlotView> ExprSlots { get; } = new();
    public List<StmtSlotView> StmtSlots { get; } = new();

    public StmtSlotView? ParentSlot = null;

    public StatementBlockView(IStatement statement, string? debugName = null)
    {
        Statement = statement;
        DebugName = debugName ?? Guid.NewGuid().ToString();

        AddToClassList("stmt-block");

        RegisterCallback<ClickEvent>(evt =>
        {
            evt.StopPropagation();
            ExpressionGraphController.Instance.SelectBlock(this);
        });

        Statement.RegisterExecutionCallback(async () => await PlayExecutePulse(100));

        BuildView();
    }

    public async Task PlayExecutePulse(int delayMs)
    {
        AddToClassList("stmt-eval");

        await Task.Delay(delayMs);

        schedule.Execute(() => { RemoveFromClassList("stmt-eval"); }).ExecuteLater(200);
    }

    void BuildView()
    {
        var title = new Label(GetStatementTitle());
        title.AddToClassList("stmt-title");
        Add(title);

        for (int i = 0; i < Statement.Arguments.Count; i++)
        {
            var arg = Statement.Arguments[i];
            var label = new Label(arg.Name + ":");
            label.AddToClassList("stmt-arg-label");
            Add(label);

            var slot = new ExprSlotView(this, i);
            ExprSlots.Add(slot);
            Add(slot);
        }

        if (Statement is IfStatement)
        {
            var doSlot = new StmtSlotView(this, StmtSlotKind.Do);
            StmtSlots.Add(doSlot);
            Add(new Label("Do:"));
            Add(doSlot);

            var elseSlot = new StmtSlotView(this, StmtSlotKind.Else);
            StmtSlots.Add(elseSlot);
            Add(new Label("Else:"));
            Add(elseSlot);
        }

        if (Statement is WhileStatement)
        {
            var bodySlot = new StmtSlotView(this, StmtSlotKind.Body);
            StmtSlots.Add(bodySlot);
            Add(new Label("Body:"));
            Add(bodySlot);
        }

        var nextSlot = new StmtSlotView(this, StmtSlotKind.Next);
        StmtSlots.Add(nextSlot);
        Add(new Label("Next:"));
        Add(nextSlot);
    }

    string GetStatementTitle()
    {
        return Statement switch
        {
            PrintStatement => "Print",
            AssignStatement assign => $"Assign ({assign.Var.Name})",
            IfStatement => "If",
            WhileStatement => "While",
            _ => "Statement"
        };
    }

    public void SetExpression(int index, Expression? expression)
    {
        if (index < 0 || index >= Statement.Arguments.Count)
            return;

        Statement.Arguments[index].Attached = expression;
    }

    public Expression? GetExpression(int index)
    {
        if (index < 0 || index >= Statement.Arguments.Count)
            return null;

        return Statement.Arguments[index].Attached;
    }

    public void SetStatementSlot(StmtSlotKind kind, StatementBlockView block)
    {
        switch (kind)
        {
            case StmtSlotKind.Next:
                Statement.Next = block.Statement;
                break;
            case StmtSlotKind.Do:
                if (Statement is IfStatement ifStmt)
                    ifStmt.Do = block.Statement;
                break;
            case StmtSlotKind.Else:
                if (Statement is IfStatement ifStmt2)
                    ifStmt2.Else = block.Statement;
                break;
            case StmtSlotKind.Body:
                if (Statement is WhileStatement whileStmt)
                    whileStmt.Body = block.Statement;
                break;
        }
    }

    public void ClearStatementSlot(StmtSlotKind kind)
    {
        switch (kind)
        {
            case StmtSlotKind.Next:
                Statement.Next = null;
                break;
            case StmtSlotKind.Do:
                if (Statement is IfStatement ifStmt)
                    ifStmt.Do = null;
                break;
            case StmtSlotKind.Else:
                if (Statement is IfStatement ifStmt2)
                    ifStmt2.Else = null;
                break;
            case StmtSlotKind.Body:
                if (Statement is WhileStatement whileStmt)
                    whileStmt.Body = null;
                break;
        }
    }

    public void AttachToSlot()
    {
        style.position = Position.Relative;
        style.left = StyleKeyword.Null;
        style.top = StyleKeyword.Null;
    }

    public void MakeFree(Vector2 pos)
    {
        style.position = Position.Absolute;

        var size = resolvedStyle;

        style.left = pos.x - size.width / 2;
        style.top = pos.y - size.height / 2;
    }
}

#nullable enable

using System;
using System.Collections.Generic;
using Script.Core.Expressions;
using Script.Core.Utils;
using Script.Core.Expressions.BinaryExpressions;
using UnityEngine.UIElements;
using UnityEngine;
using Script.Core.Expressions.LiteralExpressions.Implementations;
using Script.Core.Expressions.LiteralExpressions;
using System.Threading;
using System.Threading.Tasks;

public class ExpressionBlockView : VisualElement, IExpressionSlotHost
{
    public Expression Expression { get; }

    public List<ExprSlotView> Slots { get; } = new();

    public ExprSlotView? ParentSlot = null;

    public string DebugName { get; }

    public ExpressionBlockView(Expression expression, string? debugName = null)
    {
        DebugName = debugName ?? Guid.NewGuid().ToString();
        Expression = expression;

        AddToClassList("expr-block");

        RegisterCallback<ClickEvent>(evt =>
        {
            evt.StopPropagation();
            ExpressionGraphController.Instance.SelectBlock(this);
        });

        expression.RegisterEvaluateCallback(async () => await PlayEvaluatePulse(100));
    }

    public async Task PlayEvaluatePulse(int delayMs)
    {

        AddToClassList("expr-eval");

        await Task.Delay(delayMs);

        schedule.Execute(() => { RemoveFromClassList("expr-eval"); }).ExecuteLater(200);
    }

    public ExpressionBlockView(BinaryExpression expression, string? debugName = null) : this((Expression)expression, debugName)
    {
        BuildBinary(expression);
    }

    void BuildBinary(BinaryExpression expr)
    {
        var left = new ExprSlotView(this, 0);
        Slots.Add(left);
        Add(left);

        var op = new Label(BinaryTagOperations.GetOperatorText(expr.Tag));
        op.AddToClassList("expr-separator");
        Add(op);

        var right = new ExprSlotView(this, 1);
        Slots.Add(right);
        Add(right);
    }

    public ExpressionBlockView(NumeralExpression expr, string? debugName = null) : this((Expression)expr, debugName)
    {
        BuildUserInput(expr, "num:");
    }

    public ExpressionBlockView(LiteralExpression expr, string? debugName = null) : this((Expression)expr, debugName)
    {
        BuildUserInput(expr, "str:");
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

    void BuildUserInput(UserInputExpression expr, string prefix)
    {
        var label = new Label(prefix);
        label.AddToClassList("expr-prefix");
        Add(label);

        var field = new TextField
        {
            value = expr.RawText
        };
        field.AddToClassList("expr-input");

        field.RegisterCallback<FocusOutEvent>(_ =>
        {
            expr.RawText = field.value;
        });

        field.RegisterCallback<KeyDownEvent>(evt =>
        {
            if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter)
            {
                expr.RawText = field.value;
                field.Blur();
            }
        });

        Add(field);
    }

    public ExpressionBlockView(TrueConstant expr, string? debugName = null) : this((Expression)expr, debugName)
    {
        Add(CreateBoolLabel("true"));
    }

    public ExpressionBlockView(FalseConstant expr, string? debugName = null) : this((Expression)expr, debugName)
    {
        Add(CreateBoolLabel("false"));
    }

    public ExpressionBlockView(VariableExpression expr, string? debugName = null) : this((Expression)expr, debugName)
    {
        BuildVariable(expr);
    }

    void BuildVariable(VariableExpression expr)
    {
        var label = new Label(expr.Var.Name);
        label.AddToClassList("expr-variable");

        Add(label);
    }

    public void SetExpression(int index, Expression? expression)
    {
        Expression[index] = expression;
    }

    public Expression? GetExpression(int index)
    {
        return Expression[index];
    }

    Label CreateBoolLabel(string text)
    {
        var label = new Label(text);
        label.AddToClassList("expr-bool");
        return label;
    }

    void BuildSlots()
    {
        int arity = Expression.Arity();

        for (int i = 0; i < arity; i++)
        {
            var slot = new ExprSlotView(this, i);
            Slots.Add(slot);
            Add(slot);
        }
    }
}
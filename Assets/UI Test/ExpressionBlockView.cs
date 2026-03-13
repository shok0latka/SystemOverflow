#nullable enable

using System;
using System.Collections.Generic;
using Script.Core.Expressions;
using Script.Core.Expressions.BinaryExpressions;
using UnityEngine.UIElements;
using UnityEngine;
using Script.Core.Expressions.LiteralExpressions.Implementations;
using Script.Core.Expressions.LiteralExpressions;

public class ExpressionBlockView : VisualElement
{
    public Expression Expression { get; }

    public List<SlotView> Slots { get; } = new();

    public SlotView? ParentSlot = null;

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
    }

    public ExpressionBlockView(BinaryExpression expression, string? debugName = null) : this((Expression)expression, debugName)
    {
        BuildBinary(expression);
    }

    void BuildBinary(BinaryExpression expr)
    {
        var left = new SlotView(this, 0);
        Slots.Add(left);
        Add(left);

        var op = new Label(GetOperatorText(expr.Tag));
        op.AddToClassList("expr-separator");
        Add(op);

        var right = new SlotView(this, 1);
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
            var slot = new SlotView(this, i);
            Slots.Add(slot);
            Add(slot);
        }
    }

    static string GetOperatorText(BinaryOperatorTag tag)
    {
        return tag switch
        {
            BinaryOperatorTag.Addition => "+",
            BinaryOperatorTag.Subtraction => "-",
            BinaryOperatorTag.Multiplication => "*",
            BinaryOperatorTag.Division => "/",
            BinaryOperatorTag.Reminder => "%",
            BinaryOperatorTag.LogicalAnd => "&&",
            BinaryOperatorTag.LogicalOr => "||",
            BinaryOperatorTag.LogicalXor => "^",
            BinaryOperatorTag.GreaterThan => ">",
            BinaryOperatorTag.GreaterOrEqual => ">=",
            BinaryOperatorTag.LessThan => "<",
            BinaryOperatorTag.LessOrEqual => "<=",
            BinaryOperatorTag.Equal => "==",
            BinaryOperatorTag.NotEqual => "!=",
            _ => "?"
        };
    }
}
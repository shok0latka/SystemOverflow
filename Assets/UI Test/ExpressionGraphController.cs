#nullable enable

using System;
using Script.Core.Expressions;
using UnityEngine;
using UnityEngine.UIElements;

public class ExpressionGraphController
{
    public static ExpressionGraphController Instance = new();

    public ExpressionBlockView? SelectedBlock;
    public SlotView? SelectedSlot;

    public void SelectBlock(ExpressionBlockView block)
    {
        Debug.Log($"[GraphController] SelectBlock -> {block.DebugName}");

        if (SelectedBlock == block)
        {
            block.RemoveFromClassList("expr-selected");
            SelectedBlock = null;
            Debug.Log($"[GraphController] Block deselected");
            return;
        }

        SelectedBlock?.RemoveFromClassList("expr-selected");

        SelectedBlock = block;
        block.AddToClassList("expr-selected");

        TryConnect();
    }

    public void SelectSlot(SlotView slot)
    {
        Debug.Log($"[GraphController] SelectSlot -> {slot.ParentBlock.DebugName}[{slot.Index}]");

        if (SelectedSlot == slot)
        {
            slot.RemoveFromClassList("expr-selected");
            SelectedSlot = null;
            Debug.Log($"[GraphController] Slot deselected");
            return;
        }

        SelectedSlot?.RemoveFromClassList("expr-selected");

        SelectedSlot = slot;
        slot.AddToClassList("expr-selected");

        TryConnect();
    }

    public void ClickOnEmptySpace(Vector2 mousePosition)
    {
        if (SelectedBlock == null)
            return;

        Debug.Log($"[GraphController] Place block '{SelectedBlock.DebugName}' on canvas");

        DetachBlock(SelectedBlock);

        SelectedBlock.style.position = Position.Absolute;

        var size = SelectedBlock.resolvedStyle;

        SelectedBlock.style.left = mousePosition.x - size.width / 2;
        SelectedBlock.style.top = mousePosition.y - size.height / 2;

        SelectedBlock.RemoveFromClassList("expr-selected");
        SelectedBlock = null;
    }

    public void DetachBlock(ExpressionBlockView block)
    {
        Debug.Log($"[GraphController] DetachBlock -> {block.DebugName}");

        if (block.ParentSlot != null)
            block.ParentSlot.ClearChild();

        GraphRoot.Instance?.AddFreeBlock(block);
    }

    void TryConnect()
    {
        if (SelectedBlock == null || SelectedSlot == null)
            return;

        Debug.Log($"[GraphController] TryConnect -> {SelectedBlock.DebugName} -> {SelectedSlot.ParentBlock.DebugName}[{SelectedSlot.Index}]");

        Connect(SelectedBlock, SelectedSlot);

        SelectedSlot?.RemoveFromClassList("expr-selected");
        SelectedBlock?.RemoveFromClassList("expr-selected");

        SelectedBlock = null;
        SelectedSlot = null;
    }

    void Connect(ExpressionBlockView block, SlotView slot)
    {
        Expression parentExpr = slot.ParentBlock.Expression;
        Expression childExpr = block.Expression;

        Debug.Log($"[GraphController] Connect -> {block.DebugName} -> {slot.ParentBlock.DebugName}[{slot.Index}]");

        if (WouldCreateCycle(parentExpr, childExpr))
        {
            Debug.LogWarning("[GraphController] Connection rejected (cycle detected)");

            SelectedSlot?.RemoveFromClassList("expr-selected");
            SelectedBlock?.RemoveFromClassList("expr-selected");

            SelectedBlock = null;
            SelectedSlot = null;
            return;
        }

        if (block.ParentSlot != null && block.ParentSlot != slot)
        {
            block.ParentSlot.ClearChild();
            GraphRoot.Instance?.AddFreeBlock(block);
        }

        if (slot.ChildBlock != null)
        {
            var oldBlock = slot.ChildBlock;

            Debug.Log($"[GraphController] Replace block -> {oldBlock.DebugName}");

            slot.ClearChild();
            GraphRoot.Instance?.AddFreeBlock(oldBlock);
        }

        slot.SetChild(block);

        Debug.Log("[GraphController] Connect success");
    }

    bool WouldCreateCycle(Expression parent, Expression child)
    {
        if (parent == child)
            return true;

        int arity = child.Arity();

        for (int i = 0; i < arity; i++)
        {
            var input = child[i];

            if (input != null && WouldCreateCycle(parent, input))
                return true;
        }

        return false;
    }
}
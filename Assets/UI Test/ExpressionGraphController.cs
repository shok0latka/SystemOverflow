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

    void ToggleSelection<T>(T element, ref T? selected) where T : VisualElement
    {
        if (selected == element)
        {
            element.RemoveFromClassList("expr-selected");
            selected = null;
            return;
        }

        selected?.RemoveFromClassList("expr-selected");

        selected = element;
        element.AddToClassList("expr-selected");
    }

    public void SelectBlock(ExpressionBlockView block)
    {
        Debug.Log($"[GraphController] SelectBlock -> {block.DebugName}");

        ToggleSelection(block, ref SelectedBlock);

        TryConnect();
    }

    public void SelectSlot(SlotView slot)
    {
        Debug.Log($"[GraphController] SelectSlot -> {slot.ParentBlock.DebugName}[{slot.Index}]");

        ToggleSelection(slot, ref SelectedSlot);

        TryConnect();
    }

    public void ClickOnEmptySpace(Vector2 mousePosition)
    {
        if (SelectedBlock == null)
            return;

        Debug.Log($"[GraphController] Place block '{SelectedBlock.DebugName}' on canvas");

        DetachBlock(SelectedBlock);

        SelectedBlock.MakeFree(mousePosition);

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
            Debug.LogWarning($"[GraphController] Connection rejected: cycle detected for {block.DebugName}");
            SelectedSlot?.RemoveFromClassList("expr-selected");
            SelectedBlock?.RemoveFromClassList("expr-selected");
            SelectedBlock = null;
            SelectedSlot = null;
            return;
        }

        if (block.ParentSlot != null && block.ParentSlot != slot)
        {
            Debug.Log($"[GraphController] Detach {block.DebugName} from previous slot {block.ParentSlot.Index}");
            block.ParentSlot.ClearChild();
            GraphRoot.Instance?.AddFreeBlock(block);
        }

        var replaced = slot.ReplaceChild(block);
        if (replaced != null)
        {
            Debug.Log($"[GraphController] Slot occupied, freeing block {replaced.DebugName}");
            GraphRoot.Instance?.AddFreeBlock(replaced);
        }

        Debug.Log($"[GraphController] Connect success: {block.DebugName} -> {slot.ParentBlock.DebugName}[{slot.Index}]");
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